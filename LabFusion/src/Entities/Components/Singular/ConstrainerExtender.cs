using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class ConstrainerExtender : EntityComponentExtender<Constrainer>
{
    public static FusionComponentCache<Constrainer, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Constrainer component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Constrainer component)
    {
        Cache.Remove(component);
    }
}