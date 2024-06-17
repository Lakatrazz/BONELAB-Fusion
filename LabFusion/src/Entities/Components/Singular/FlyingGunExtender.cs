using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class FlyingGunExtender : EntityComponentExtender<FlyingGun>
{
    public static FusionComponentCache<FlyingGun, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, FlyingGun component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, FlyingGun component)
    {
        Cache.Remove(component);
    }
}