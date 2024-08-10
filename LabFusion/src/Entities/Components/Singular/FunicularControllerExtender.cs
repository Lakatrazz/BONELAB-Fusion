using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class FunicularControllerExtender : EntityComponentExtender<FunicularController>
{
    public static FusionComponentCache<FunicularController, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, FunicularController component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, FunicularController component)
    {
        Cache.Remove(component);
    }
}