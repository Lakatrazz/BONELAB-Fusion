using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Extenders;

public class PropFlashlightExtender : EntityComponentExtender<PropFlashlight>
{
    public static readonly FusionComponentCache<PropFlashlight, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, PropFlashlight component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, PropFlashlight component)
    {
        Cache.Remove(component);
    }
}