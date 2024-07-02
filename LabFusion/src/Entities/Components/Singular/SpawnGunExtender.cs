using LabFusion.Utilities;
using LabFusion.MonoBehaviours;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Interaction;

using UnityEngine;

namespace LabFusion.Entities;

public class SpawnGunExtender : EntityComponentExtender<SpawnGun>
{
    public static readonly FusionComponentCache<SpawnGun, NetworkEntity> Cache = new();

    private TimedDespawner _despawner = null;

    protected override void OnRegister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Add(component, networkEntity);

        RegisterDespawner(component._poolee);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Remove(component);

        UnregisterDespawner();
    }

    private void RegisterDespawner(Poolee poolee)
    {
        _despawner = poolee.gameObject.AddComponent<TimedDespawner>();
        _despawner.Poolee = poolee;
    }

    private void UnregisterDespawner()
    {
        GameObject.Destroy(_despawner);
        _despawner = null;
    }
    
    private void OnHandAttached(InteractableHost host, Hand hand)
    {

    }

    private void OnHandDetached(InteractableHost host, Hand hand)
    {

    }

    private void ToggleDespawner(bool enabled)
    {
        _despawner.enabled = enabled;
        _despawner.RefreshTimer();
    }
}