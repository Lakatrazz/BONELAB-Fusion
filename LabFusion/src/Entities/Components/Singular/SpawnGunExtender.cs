using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class SpawnGunExtender : EntityComponentExtender<SpawnGun>
{
    public static FusionComponentCache<SpawnGun, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, SpawnGun component)
    {
        Cache.Remove(component);
    }
}