using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class KeyRecieverExtender : EntityComponentArrayExtender<KeyReceiver>
{
    public static FusionComponentCache<KeyReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, KeyReceiver[] components)
    {
        foreach (var key in components)
        {
            Cache.Add(key, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, KeyReceiver[] components)
    {
        foreach (var key in components)
        {
            Cache.Remove(key);
        }
    }
}