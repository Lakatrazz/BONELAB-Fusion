using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class AmmoSocketExtender : EntityComponentExtender<AmmoSocket>
{
    public static FusionComponentCache<AmmoSocket, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, AmmoSocket component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, AmmoSocket component)
    {
        Cache.Remove(component);
    }
}