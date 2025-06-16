using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.MonoBehaviours;

using UnityEngine;

namespace LabFusion.Entities;

using System;

/// <summary>
/// Attaches to a NetworkEntity and automatically cleans it up when not interacted with for a certain amount of time.
/// </summary>
public class EntityCleaner
{
    public TimedDespawner Despawner { get; set; } = null;

    public InteractableHost Host { get; set; } = null;

    public bool Registered { get; set; } = false;

    public NetworkEntity NetworkEntity { get; set; } = null;

    private Il2CppSystem.Action<InteractableHost, Hand> _onHandAttached = null;
    private Il2CppSystem.Action<InteractableHost, Hand> _onHandDetached = null;

    public void Register(NetworkEntity networkEntity, InteractableHost host, Poolee poolee)
    {
        if (Registered)
        {
            return;
        }

        if (poolee == null)
        {
            return;
        }

        NetworkEntity = networkEntity;

        Host = host;

        RegisterGrips(host);
        RegisterDespawner(poolee);

        Registered = true;
    }

    public void Unregister()
    {
        if (!Registered)
        {
            return;
        }

        UnregisterGrips(Host);
        UnregisterDespawner();

        NetworkEntity = null;

        Host = null;

        Registered = false;
    }

    private void RegisterGrips(InteractableHost host)
    {
        _onHandAttached = (Action<InteractableHost, Hand>)OnHandAttached;
        _onHandDetached = (Action<InteractableHost, Hand>)OnHandDetached;

        host.onHandAttachedDelegate += _onHandAttached;
        host.onHandDetachedDelegate += _onHandDetached;
    }

    private void UnregisterGrips(InteractableHost host)
    {
        host.onHandAttachedDelegate -= _onHandAttached;
        host.onHandDetachedDelegate -= _onHandDetached;

        _onHandAttached = null;
        _onHandDetached = null;
    }

    private void RegisterDespawner(Poolee poolee)
    {
        Despawner = poolee.gameObject.AddComponent<TimedDespawner>();
        Despawner.Poolee = poolee;
        Despawner.OnDespawnCheck += OnDespawnCheck;
    }

    private void UnregisterDespawner()
    {
        Despawner.OnDespawnCheck -= OnDespawnCheck;
        Despawner.Poolee = null;

        GameObject.Destroy(Despawner);
        Despawner = null;
    }

    private bool OnDespawnCheck()
    {
        // Only automatically despawn if we own the entity
        if (NetworkEntity != null && !NetworkEntity.IsOwner)
        {
            return false;
        }

        var marrowBody = Host._marrowBody;

        // If there's no marrow body, it probably shouldn't be despawned anyways
        if (!marrowBody)
        {
            return false;
        }

        // Make sure the rigidbody hasn't been destroyed
        if (!marrowBody.HasRigidbody)
        {
            return false;
        }

        // If it's kinematic, it's likely holstered
        if (marrowBody._rigidbody.isKinematic)
        {
            return false;
        }

        return true;
    }

    private int _handCount = 0;

    private void OnHandAttached(InteractableHost host, Hand hand)
    {
        _handCount = Math.Max(_handCount, 0);
        _handCount++;

        // We know at least one hand has started grabbing, so disable the despawner
        ToggleDespawner(false);
    }

    private void OnHandDetached(InteractableHost host, Hand hand)
    {
        _handCount--;
        _handCount = Math.Max(_handCount, 0);

        // All hands have let go
        if (_handCount <= 0)
        {
            ToggleDespawner(true);
        }
    }

    private void ToggleDespawner(bool enabled)
    {
        Despawner.enabled = enabled;
        Despawner.RefreshTimer();
    }
}