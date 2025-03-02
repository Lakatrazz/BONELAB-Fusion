using Il2CppSLZ.Marrow;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class InventorySlotReceiverExtender : EntityComponentArrayExtender<InventorySlotReceiver>
{
    public static readonly FusionComponentCache<InventorySlotReceiver, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, InventorySlotReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }

        networkEntity.OnEntityCatchup += OnEntityCatchup;
    }

    protected override void OnUnregister(NetworkEntity networkEntity, InventorySlotReceiver[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        networkEntity.OnEntityCatchup -= OnEntityCatchup;
    }

    private void OnEntityCatchup(NetworkEntity entity, PlayerId player)
    {
        foreach (var component in Components)
        {
            OnEntityCatchup(component, entity, player);
        }
    }

    private void OnEntityCatchup(InventorySlotReceiver receiver, NetworkEntity entity, PlayerId player)
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

        var data = InventorySlotInsertData.Create(entity.Id, weaponEntity.Id, index.Value);

        MessageRelay.RelayNative(data, NativeMessageTag.InventorySlotInsert, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}