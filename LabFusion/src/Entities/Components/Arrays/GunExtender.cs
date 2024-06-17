using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class GunExtender : EntityComponentArrayExtender<Gun>
{
    public static FusionComponentCache<Gun, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Gun[] components)
    {
        foreach (var gun in components)
        {
            Cache.Add(gun, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Gun[] components)
    {
        foreach (var gun in components)
        {
            Cache.Remove(gun);
        }
    }
}