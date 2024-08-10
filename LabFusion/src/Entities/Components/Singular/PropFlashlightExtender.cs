using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class PropFlashlightExtender : EntityComponentExtender<PropFlashlight>
{
    public static FusionComponentCache<PropFlashlight, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, PropFlashlight component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, PropFlashlight component)
    {
        Cache.Remove(component);
    }
}