using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class GunExtender : EntityComponentArrayExtender<Gun>
{
    public static FusionComponentCache<Gun, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, Gun[] components)
    {
        foreach (var gun in components)
        {
            Cache.Add(gun, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Gun[] components)
    {
        foreach (var gun in components)
        {
            Cache.Remove(gun);
        }
    }
}