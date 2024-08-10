using LabFusion.Utilities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Entities;

public class AIBrainExtender : EntityComponentExtender<AIBrain>
{
    public static FusionComponentCache<AIBrain, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, AIBrain component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, AIBrain component)
    {
        Cache.Remove(component);
    }
}