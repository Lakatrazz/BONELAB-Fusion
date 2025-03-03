using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class KeyReceiverExtender : EntityComponentArrayExtender<KeyReceiver>
{
    public static readonly FusionComponentCache<KeyReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, KeyReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, KeyReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}