using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow.Extenders;

public class CrateSpawnerExtender : EntityComponentArrayExtender<CrateSpawner>
{
    public static readonly FusionComponentCache<CrateSpawner, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, CrateSpawner[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, CrateSpawner[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}