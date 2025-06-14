using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class GunExtender : EntityComponentArrayExtender<Gun>
{
    public static readonly FusionComponentCache<Gun, NetworkEntity> Cache = new();

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