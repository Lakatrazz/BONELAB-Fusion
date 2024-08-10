using LabFusion.Utilities;
using LabFusion.Network;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class SpawnGunExtender : EntityComponentExtender<SpawnGun>
{
    public static readonly FusionComponentCache<SpawnGun, NetworkEntity> Cache = new();

    private TimedDespawnHandler _despawnHandler = null;

    protected override void OnRegister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Add(component, networkEntity);

        if (NetworkInfo.IsServer)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component.host, component._poolee);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Remove(component);

        if (_despawnHandler != null)
        {
            _despawnHandler.Unregister();
            _despawnHandler = null;
        }
    }
}