using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.MonoBehaviours;

using UnityEngine;

namespace LabFusion.Entities;

public class TimedDespawnHandler
{
    private TimedDespawner _despawner = null;

    private InteractableHost _host = null;

    private bool _registered = false;

    private Il2CppSystem.Action<InteractableHost, Hand> _onHandAttached = null;
    private Il2CppSystem.Action<InteractableHost, Hand> _onHandDetached = null;

    public void Register(InteractableHost host, Poolee poolee)
    {
        if (_registered)
        {
            return;
        }

        if (poolee == null)
        {
            return;
        }

        _host = host;

        RegisterGrips(host);
        RegisterDespawner(poolee);

        _registered = true;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterGrips(_host);
        UnregisterDespawner();

        _host = null;

        _registered = false;
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
        _despawner = poolee.gameObject.AddComponent<TimedDespawner>();
        _despawner.Poolee = poolee;
        _despawner.OnDespawnCheck += OnDespawnCheck;
    }

    private void UnregisterDespawner()
    {
        _despawner.OnDespawnCheck -= OnDespawnCheck;
        _despawner.Poolee = null;

        GameObject.Destroy(_despawner);
        _despawner = null;
    }

    private bool OnDespawnCheck()
    {
        var marrowBody = _host._marrowBody;

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
        _despawner.enabled = enabled;
        _despawner.RefreshTimer();
    }
}