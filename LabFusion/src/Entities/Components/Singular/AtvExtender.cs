using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Vehicle;

namespace LabFusion.Entities;

public class AtvExtender : EntityComponentExtender<Atv>
{
    public static FusionComponentCache<Atv, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Atv component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Atv component)
    {
        Cache.Remove(component);
    }
}
