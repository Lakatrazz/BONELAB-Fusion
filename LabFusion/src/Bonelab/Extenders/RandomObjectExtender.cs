using Il2CppSLZ.Bonelab;

using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Extenders;

public class RandomObjectExtender : EntityComponentArrayExtender<RandomObject>
{
    public static readonly FusionComponentCache<RandomObject, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, RandomObject[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, RandomObject[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}