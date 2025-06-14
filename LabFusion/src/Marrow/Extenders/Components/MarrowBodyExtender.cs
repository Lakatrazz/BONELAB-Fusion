using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.Marrow.Extenders;

public class MarrowBodyExtender : EntityComponentArrayExtender<MarrowBody>
{
    public static readonly FusionComponentCache<MarrowBody, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, MarrowBody[] components)
    {
        foreach (var body in components)
        {
            Cache.Add(body, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, MarrowBody[] components)
    {
        foreach (var body in components)
        {
            Cache.Remove(body);
        }
    }
}