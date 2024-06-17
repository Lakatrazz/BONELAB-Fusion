using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Entities;

public class EscalatorExtender : EntityComponentExtender<Escalator>
{
    public static FusionComponentCache<Escalator, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Escalator component)
    {
        Cache.Add(component, networkEntity);

        // TODO Add back sync disabling
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Escalator component)
    {
        Cache.Remove(component);
    }
}