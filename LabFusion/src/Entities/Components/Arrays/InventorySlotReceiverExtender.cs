using Il2CppSLZ.Marrow;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public class InventorySlotReceiverExtender : EntityComponentArrayExtender<InventorySlotReceiver>
{
    public static readonly FusionComponentCache<InventorySlotReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, InventorySlotReceiver[] components)
    {
        foreach (var slot in components)
        {
            Cache.Add(slot, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, InventorySlotReceiver[] components)
    {
        foreach (var slot in components)
        {
            Cache.Remove(slot);
        }
    }
}