using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class WeaponSlotExtender : EntityComponentExtender<WeaponSlot>
{
    public static readonly FusionComponentCache<WeaponSlot, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, WeaponSlot component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, WeaponSlot component)
    {
        Cache.Remove(component);
    }
}
