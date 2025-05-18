using Il2CppCysharp.Threading.Tasks;
using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.SDK.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class GamemodeItem : MonoBehaviour
{
    public GamemodeItem(IntPtr intPtr) : base(intPtr) { }

    public const float MaxUnusedTime = 20f;

    private NetworkEntity _entity = null;
    private NetworkProp _prop = null;
    private Poolee _poolee = null;

    private InteractableHost _host = null;
    private InteractableHostManager _hostManager = null;

    private MarrowEntity _marrowEntity = null;

    private GunExtender _gunExtender = null;

    private bool _isDespawned = false;

    private Il2CppSystem.Action<InteractableHost, Hand> _onHandAttachedDelegate = null;

    public bool IsHeld => _host != null && _host.HandCount() > 0;

    public bool HasBeenInteracted { get; private set; } = false;

    [HideFromIl2Cpp]
    public void Initialize(NetworkEntity entity, Poolee poolee)
    {
        _entity = entity;
        _poolee = poolee;

        _prop = entity.GetExtender<NetworkProp>();

        if (_prop != null)
        {
            _marrowEntity = _prop.MarrowEntity;

            HookHost();
        }

        CheckExtenders(entity);

        GamemodeDropper.DroppedItems.Add(poolee);
    }

    private void CheckExtenders(NetworkEntity entity)
    {
        var gunExtender = entity.GetExtender<GunExtender>();

        if (gunExtender != null)
        {
            HookGun(gunExtender);
        }
    }

    private void HookGun(GunExtender gunExtender)
    {
        _gunExtender = gunExtender;

        foreach (var gun in gunExtender.Components)
        {
            var awaiter = gun.InstantLoadAsync().GetAwaiter();

            var continuation = () =>
            {
                gun.Charge();

                gun.hammerState = Gun.HammerStates.COCKED;
                gun.slideState = Gun.SlideStates.RETURNED;

                gun.isMagEjectOnEmpty = true;
            };

            awaiter.OnCompleted(continuation);
        }
    }

    private void HookHost()
    {
        _host = InteractableHost.Cache.Get(_marrowEntity.gameObject);
        _hostManager = _marrowEntity.gameObject.GetComponent<InteractableHostManager>();

        _onHandAttachedDelegate = (Action<InteractableHost, Hand>)OnHandAttached;

        if (_hostManager != null)
        {
            _hostManager.onHandAttached += _onHandAttachedDelegate;
        }
        else if (_host != null)
        {
            _host.onHandAttachedDelegate += _onHandAttachedDelegate;
        }
    }

    private void UnhookHost()
    {
        if (_hostManager != null)
        {
            _hostManager.onHandAttached -= _onHandAttachedDelegate;
        }
        else if (_host != null)
        {
            _host.onHandAttachedDelegate -= _onHandAttachedDelegate;
        }

        _onHandAttachedDelegate = null;

        _host = null;
        _hostManager = null;
    }

    private void OnHandAttached(InteractableHost host, Hand hand)
    {
        HasBeenInteracted = true;

        if (hand.manager.IsLocalPlayer())
        {
            OnGunGrabbed();
        }
    }

    private void OnGunGrabbed()
    {
        if (_gunExtender == null)
        {
            return;
        }

        foreach (var gun in _gunExtender.Components)
        {
            if (!gun._hasMagState)
            {
                continue;
            }

            LocalInventory.AddAmmo(gun.MagazineState._cartridges.Count);
        }
    }

    private void OnDespawn()
    {
        if (_isDespawned)
        {
            return;
        }

        _isDespawned = true;
        HasBeenInteracted = false;
        _despawnedGun = false;

        GamemodeDropper.DroppedItems.Remove(_poolee);

        UnhookHost();

        _entity = null;
        _prop = null;
        _poolee = null;

        Destroy(this);
    }

    private void OnDisable()
    {
        OnDespawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HasBeenInteracted)
        {
            return;
        }

        // Disable low gravity on player collision
        if (collision.collider.gameObject.layer == (int)MarrowLayers.Player)
        {
            HasBeenInteracted = true;
        }
    }

    private void Update()
    {
        if (NetworkInfo.IsServer)
        {
            CheckTimedDespawn();
        }

        CheckGunDespawn();
    }

    private void FixedUpdate()
    {
        ApplyLowGravity();
    }

    private void ApplyLowGravity()
    {
        if (HasBeenInteracted)
        {
            return;
        }

        if (_marrowEntity == null || _marrowEntity.Bodies.Count <= 0)
        {
            return;
        }

        var marrowBody = _marrowEntity.Bodies[0];

        if (marrowBody == null || !marrowBody.HasRigidbody)
        {
            return;
        }

        var rigidbody = marrowBody._rigidbody;

        rigidbody.AddForce(-Physics.gravity * 0.9f, ForceMode.Acceleration);
    }

    private float _despawnTimer = 0f;

    private void CheckTimedDespawn()
    {
        if (IsHeld)
        {
            _despawnTimer = 0f;
            return;
        }

        _despawnTimer += Time.deltaTime;

        if (_despawnTimer >= MaxUnusedTime)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityId = _entity.Id,
                DespawnEffect = HasBeenInteracted,
            });
        }
    }

    private bool _despawnedGun = false;

    private void CheckGunDespawn()
    {
        if (_despawnedGun)
        {
            return;
        }

        if (!HasBeenInteracted)
        {
            return;
        }

        if (!_entity.IsOwner)
        {
            return;
        }

        if (_gunExtender == null)
        {
            return;
        }

        bool isLoaded = false;

        foreach (var gun in _gunExtender.Components)
        {
            if (gun.HasMagazine() && gun.MagazineState.AmmoCount > 0)
            {
                isLoaded = true;
            }
        }

        if (!isLoaded)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityId = _entity.Id,
                DespawnEffect = true,
            });

            _despawnedGun = true;
        }
    }
}
