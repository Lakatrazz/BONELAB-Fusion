using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class WeaponSlotExtender : EntityComponentExtender<WeaponSlot>
{
    public static FusionComponentCache<WeaponSlot, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, WeaponSlot component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, WeaponSlot component)
    {
        Cache.Remove(component);
    }
}
