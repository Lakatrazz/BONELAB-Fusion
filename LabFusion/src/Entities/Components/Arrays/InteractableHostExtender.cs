using Il2CppSLZ.Marrow;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public class InteractableHostExtender : EntityComponentArrayExtender<InteractableHost>
{
    public static readonly FusionComponentCache<InteractableHost, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Add(host, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Remove(host);
        }
    }
}