using Il2CppSLZ.Marrow;

using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.Marrow.Extenders;

public class InteractableHostExtender : EntityComponentArrayExtender<InteractableHost>
{
    public static readonly FusionComponentCache<InteractableHost, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Add(host, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Remove(host);
        }
    }
}