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
using LabFusion.Marrow.Extenders;

using MelonLoader;

using UnityEngine;

namespace LabFusion.SDK.MonoBehaviours;

/// <summary>
/// Custom logic applied to dropped items for Gamemodes.
/// </summary>
[RegisterTypeInIl2Cpp]
public class GamemodeItem : MonoBehaviour
{
    public GamemodeItem(IntPtr intPtr) : base(intPtr) { }

    public const float MaxUnheldTime = 20f;

    public const float MaxHeldTime = 60f;

    public const int MaxMeleeHits = 10;

    public const int MaxDamagelessHits = MaxMeleeHits * 3;

    private NetworkEntity _entity = null;
    private NetworkProp _prop = null;
    private Poolee _poolee = null;

    private InteractableHost _host = null;
    private InteractableHostManager _hostManager = null;

    private MarrowEntity _marrowEntity = null;

    private GunExtender _gunExtender = null;

    private StabSlash _stabSlash = null;
    private StabSlash.BladeAudio _bladeAudio = null;

    private ImpactSFX _impactSFX = null;
    private Il2CppSystem.Action<Collision, float> _onSignificantCollisionDelegate = null;

    private bool _isDespawned = false;

    private Il2CppSystem.Action<InteractableHost, Hand> _onHandAttachedDelegate = null;

    public bool IsHeld
    {
        get
        {
            if (_hostManager)
            {
                return _hostManager.grabbedHosts.Count > 0;
            }

            if (_host)
            {
                return _host.HandCount() > 0;
            }

            return false;
        }
    }

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
            CheckComponents(_marrowEntity);
        }

        CheckExtenders(entity);

        GamemodeDropper.DroppedItems.Add(poolee);
    }

    [HideFromIl2Cpp]
    private void CheckExtenders(NetworkEntity entity)
    {
        var gunExtender = entity.GetExtender<GunExtender>();

        if (gunExtender != null)
        {
            HookGun(gunExtender);
        }
    }

    private void CheckComponents(MarrowEntity marrowEntity)
    {
        _stabSlash = marrowEntity.GetComponent<StabSlash>();

        if (_stabSlash != null)
        {
            _bladeAudio = _stabSlash.bladeAudio;
        }

        _impactSFX = marrowEntity.GetComponent<ImpactSFX>();

        if (_impactSFX != null)
        {
            _onSignificantCollisionDelegate = (Action<Collision, float>)OnImpactSFXCollision;
            _impactSFX.OnSignificantCollision += _onSignificantCollisionDelegate;
        }
    }

    [HideFromIl2Cpp]
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

                gun.isMagEjectOnEmpty = true; // This prevents ejecting the mag in any way, I don't care enough to reset this so rip pooling in gamemode maps ig
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

    private void UnhookComponents()
    {
        if (_impactSFX != null)
        {
            _impactSFX.OnSignificantCollision -= _onSignificantCollisionDelegate;
            _onSignificantCollisionDelegate = null;
            _impactSFX = null;
        }
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
        _attemptedDespawn = false;

        GamemodeDropper.DroppedItems.Remove(_poolee);

        UnhookHost();
        UnhookComponents();

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
        if (NetworkInfo.IsHost)
        {
            CheckTimedDespawn();
            CheckHeldDespawn();
        }

        CheckGunDespawn();
    }

    private void FixedUpdate()
    {
        ApplyLowGravity();

        CheckStabSlashDespawn();
    }

    private void ApplyLowGravity()
    {
        if (HasBeenInteracted)
        {
            return;
        }

        if (_marrowEntity == null)
        {
            return;
        }

        var antiGravity = -Physics.gravity * 0.9f;

        foreach (var body in _marrowEntity.Bodies)
        {
            if (body == null || !body.HasRigidbody)
            {
                continue;
            }

            var rigidbody = body._rigidbody;

            if (!rigidbody.useGravity)
            {
                continue;
            }

            rigidbody.AddForce(antiGravity, ForceMode.Acceleration);
        }
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

        if (_despawnTimer >= MaxUnheldTime)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityID = _entity.ID,
                DespawnEffect = HasBeenInteracted,
            });
        }
    }

    private float _heldDespawnTimer = 0f;

    private void CheckHeldDespawn()
    {
        if (!IsHeld)
        {
            return;
        }

        _heldDespawnTimer += Time.deltaTime;

        if (_heldDespawnTimer >= MaxHeldTime)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityID = _entity.ID,
                DespawnEffect = true,
            });
        }
    }

    private bool CanCheckItemUse()
    {
        return HasBeenInteracted && _entity.IsOwner && !_attemptedDespawn;
    }

    private bool _attemptedDespawn = false;

    private void CheckGunDespawn()
    {
        if (!CanCheckItemUse())
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
            DespawnFromItemUse();
        }
    }

    private float _lastImpactTime = 0f;
    private int _bladeHits = 0;
    private float _bladeHitCooldown = 0f;

    private void CheckStabSlashDespawn()
    {
        if (_bladeAudio == null)
        {
            return;
        }

        if (!CanCheckItemUse())
        {
            _lastImpactTime = _bladeAudio._nextImpactTime;
            return;
        }

        if (_bladeHitCooldown > 0f)
        {
            _bladeHitCooldown -= Time.deltaTime;

            _lastImpactTime = _bladeAudio._nextImpactTime;
            return;
        }

        bool bladeHit = !Mathf.Approximately(_bladeAudio._nextImpactTime, _lastImpactTime);

        bool hitRigidbody = _bladeAudio._relCol.collider != null && _bladeAudio._relCol.collider.attachedRigidbody != null;

        if (bladeHit && hitRigidbody)
        {
            _bladeHits++;
            _bladeHitCooldown = 0.1f;

            if (_bladeHits >= MaxMeleeHits)
            {
                DespawnFromItemUse();
            }

            _lastImpactTime = _bladeAudio._nextImpactTime;
        }
    }

    private int _impactSFXHits = 0;
    private float _lastImpactSFXTime = 0f;

    private void OnImpactSFXCollision(Collision collision, float velSquared)
    {
        if (!CanCheckItemUse())
        {
            return;
        }

        if (Time.timeSinceLevelLoad - _lastImpactSFXTime < 0.1f)
        {
            return;
        }

        if (velSquared < _impactSFX._minVelSquared)
        {
            return;
        }

        var maxHits = _impactSFX.bluntAttack ? MaxMeleeHits : MaxDamagelessHits;

        bool hitRigidbody = collision.rigidbody != null;

        if (hitRigidbody)
        {
            _impactSFXHits++;
            _lastImpactSFXTime = Time.timeSinceLevelLoad;

            if (_impactSFXHits >= maxHits)
            {
                DespawnFromItemUse();
            }
        }
    }

    private void DespawnFromItemUse()
    {
        NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
        {
            EntityID = _entity.ID,
            DespawnEffect = true,
        });

        _attemptedDespawn = true;
    }
}
