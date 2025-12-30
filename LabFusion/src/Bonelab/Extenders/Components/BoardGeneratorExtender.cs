using Il2CppSLZ.Bonelab;

using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Extenders;

public class BoardGeneratorExtender : EntityComponentExtender<BoardGenerator>
{
    public static readonly FusionComponentCache<BoardGenerator, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, BoardGenerator component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, BoardGenerator component)
    {
        Cache.Remove(component);
    }
}