using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class KeyRecieverExtender : EntityComponentArrayExtender<KeyReceiver>
{
    public static FusionComponentCache<KeyReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, KeyReceiver[] components)
    {
        foreach (var key in components)
        {
            Cache.Add(key, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, KeyReceiver[] components)
    {
        foreach (var key in components)
        {
            Cache.Remove(key);
        }
    }
}