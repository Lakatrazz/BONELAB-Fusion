using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Extenders;

public class AmmoPlugExtender : EntityComponentExtender<AmmoPlug>
{
    public static readonly FusionComponentCache<AmmoPlug, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AmmoPlug component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, AmmoPlug component)
    {
        Cache.Remove(component);
    }
}