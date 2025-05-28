using Il2CppSLZ.Marrow;

using LabFusion.Marrow.Messages;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.Marrow.Extenders;

public class InventorySlotReceiverExtender : EntityComponentArrayExtender<InventorySlotReceiver>
{
    public static readonly FusionComponentCache<InventorySlotReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, InventorySlotReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, InventorySlotReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        foreach (var component in Components)
        {
            OnEntityCatchup(component, entity, player);
        }
    }

    private void OnEntityCatchup(InventorySlotReceiver receiver, NetworkEntity entity, PlayerID player)
    {
        if (receiver._slottedWeapon == null)
        {
            return;
        }

        var weaponEntity = WeaponSlotExtender.Cache.Get(receiver._slottedWeapon);

        if (weaponEntity == null)
        {
            return;
        }

        byte? index = (byte?)GetIndex(receiver);

        if (!index.HasValue)
        {
            return;
        }

        var data = new InventorySlotInsertData()
        {
            SlotEntityID = entity.ID,
            WeaponID = weaponEntity.ID,
            SlotIndex = index.Value,
        };

        MessageRelay.RelayModule<InventorySlotInsertMessage, InventorySlotInsertData>(data, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}