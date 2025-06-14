using LabFusion.Marrow.Integration;
using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.SDK.Extenders;

public class VoiceProxyExtender : EntityComponentArrayExtender<VoiceProxy>
{
    public static readonly FusionComponentCache<VoiceProxy, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, VoiceProxy[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);

            component.HasNetworkEntity = true;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, VoiceProxy[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);

            component.HasNetworkEntity = false;
        }
    }
}