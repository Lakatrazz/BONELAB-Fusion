using Il2CppSLZ.Bonelab;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public class BoardGeneratorExtender : EntityComponentExtender<BoardGenerator>
{
    public static readonly FusionComponentCache<BoardGenerator, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, BoardGenerator component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, BoardGenerator component)
    {
        Cache.Remove(component);
    }
}