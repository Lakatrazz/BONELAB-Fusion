using Il2CppSLZ.Marrow;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

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

        var data = InventorySlotInsertData.Create(entity.ID, weaponEntity.ID, index.Value);

        MessageRelay.RelayNative(data, NativeMessageTag.InventorySlotInsert, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}