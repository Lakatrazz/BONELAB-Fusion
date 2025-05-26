using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(InventorySlotReceiver))]
public class InventorySlotReceiverPatches
{
    public static bool IgnorePatches = false;

    private static void OnDropWeapon(InventorySlotReceiver __instance, Hand hand = null)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._slottedWeapon)
        {
            return;
        }

        if (!InventorySlotReceiverExtender.Cache.TryGet(__instance, out var slotEntity))
        {
            return;
        }

        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

        byte? index = (byte?)slotExtender.GetIndex(__instance);

        if (!index.HasValue)
        {
            return;
        }

        // Check if we're taking from someone else's holster for the achievement
        var handedness = Handedness.UNDEFINED;

        if (hand != null)
        {
            handedness = hand.handedness;

            // Reward achievement
            if (!slotEntity.IsOwner && AchievementManager.TryGetAchievement<HighwayMan>(out var achievement))
            {
                achievement.IncrementTask();
            }
        }

        // Send a receiver drop message
        var data = InventorySlotDropData.Create(slotEntity.ID, PlayerIDManager.LocalSmallID, index.Value, handedness);

        MessageRelay.RelayNative(data, NativeMessageTag.InventorySlotDrop, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventorySlotReceiver.DropWeapon))]
    public static void DropWeaponPrefix(InventorySlotReceiver __instance)
    {
        OnDropWeapon(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventorySlotReceiver.OnHandGrab))]
    public static void OnHandGrabPrefix(InventorySlotReceiver __instance, Hand hand)
    {
        OnDropWeapon(__instance, hand);
    }
}

[HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandDrop))]
public class InventorySlotReceiverDrop
{
    public static bool PreventInsertCheck = false;

    public static void Postfix(InventorySlotReceiver __instance, IGrippable host)
    {
        if (PreventInsertCheck)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._slottedWeapon)
        {
            return;
        }

        var weaponEntity = WeaponSlotExtender.Cache.Get(__instance._slottedWeapon);

        if (weaponEntity == null)
        {
            return;
        }

        if (!InventorySlotReceiverExtender.Cache.TryGet(__instance, out var slotEntity))
        {
            return;
        }

        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

        byte? index = (byte?)slotExtender.GetIndex(__instance);

        if (!index.HasValue)
        {
            return;
        }

        var data = InventorySlotInsertData.Create(slotEntity.ID, weaponEntity.ID, index.Value);

        MessageRelay.RelayNative(data, NativeMessageTag.InventorySlotInsert, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}