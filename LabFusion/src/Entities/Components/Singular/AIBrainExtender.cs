using LabFusion.Utilities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Entities;

public class AIBrainExtender : EntityComponentExtender<AIBrain>
{
    public static FusionComponentCache<AIBrain, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AIBrain component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, AIBrain component)
    {
        Cache.Remove(component);
    }
}