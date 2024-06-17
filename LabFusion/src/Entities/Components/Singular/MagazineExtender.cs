using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class MagazineExtender : EntityComponentExtender<Magazine>
{
    public static FusionComponentCache<Magazine, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Magazine component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Magazine component)
    {
        Cache.Remove(component);
    }
}