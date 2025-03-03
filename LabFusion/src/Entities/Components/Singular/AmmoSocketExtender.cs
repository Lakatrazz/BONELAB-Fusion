using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class AmmoSocketExtender : EntityComponentExtender<AmmoSocket>
{
    public static FusionComponentCache<AmmoSocket, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AmmoSocket component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, AmmoSocket component)
    {
        Cache.Remove(component);
    }
}