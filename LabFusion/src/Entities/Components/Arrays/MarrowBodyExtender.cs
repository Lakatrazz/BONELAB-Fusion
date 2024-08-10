using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public class MarrowBodyExtender : EntityComponentArrayExtender<MarrowBody>
{
    public static readonly FusionComponentCache<MarrowBody, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, MarrowBody[] components)
    {
        foreach (var body in components)
        {
            Cache.Add(body, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, MarrowBody[] components)
    {
        foreach (var body in components)
        {
            Cache.Remove(body);
        }
    }
}