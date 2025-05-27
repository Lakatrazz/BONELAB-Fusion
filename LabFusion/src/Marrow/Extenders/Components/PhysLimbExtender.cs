using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Extenders;

public class PhysLimbExtender : EntityComponentArrayExtender<PhysLimb>
{
    public static readonly FusionComponentCache<PhysLimb, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, PhysLimb[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, PhysLimb[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}
