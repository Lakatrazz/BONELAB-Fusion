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

        // Send a receiver grab message
        using var writer = FusionWriter.Create(InventorySlotDropData.Size);
        var data = InventorySlotDropData.Create(slotEntity.Id, PlayerIdManager.LocalSmallId, index.Value, handedness);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotDrop, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
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

        using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
        var data = InventorySlotInsertData.Create(slotEntity.Id, PlayerIdManager.LocalSmallId, weaponEntity.Id, index.Value);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}