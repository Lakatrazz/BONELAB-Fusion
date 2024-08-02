using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class InventoryAmmoReceiverExtender : EntityComponentExtender<InventoryAmmoReceiver>
{
    public static readonly FusionComponentCache<InventoryAmmoReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, InventoryAmmoReceiver component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, InventoryAmmoReceiver component)
    {
        Cache.Remove(component);
    }
}