using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;
using LabFusion.Entities;
using LabFusion.Scene;
using LabFusion.Utilities;
using LabFusion.RPC;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;

using Il2CppCysharp.Threading.Tasks;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(InventorySlotReceiver))]
public class InventorySlotReceiverPatches
{
    public static bool IgnorePatches { get; set; } = false;

    private static void OnDropWeapon(InventorySlotReceiver __instance, Hand hand = null)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
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
        var data = new InventorySlotDropData() { SlotEntityID = slotEntity.ID, GrabberID = PlayerIDManager.LocalSmallID, SlotIndex = index.Value, Handedness = handedness };

        MessageRelay.RelayModule<InventorySlotDropMessage, InventorySlotDropData>(data, CommonMessageRoutes.ReliableToOtherClients);
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventorySlotReceiver.OnHandDrop))]
    public static bool OnHandDropPrefix() 
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (LocalControls.DisableInteraction || LocalControls.DisableInventory)
        {
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventorySlotReceiver.OnHandDrop))]
    public static void OnHandDropPostfix(InventorySlotReceiver __instance, IGrippable host)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
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

        var data = new InventorySlotInsertData()
        {
            SlotEntityID = slotEntity.ID,
            WeaponID = weaponEntity.ID,
            SlotIndex = index.Value,
        };

        MessageRelay.RelayModule<InventorySlotInsertMessage, InventorySlotInsertData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventorySlotReceiver.SpawnInSlotAsync))]
    public static bool SpawnInSlotAsyncPrefix(InventorySlotReceiver __instance, Barcode barcode, ref UniTask<bool> __result)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        __result = new UniTask<bool>(false);

        FusionSceneManager.HookOnLevelLoad(() =>
        {
            SpawnInSlotAsyncOnLevelLoad(__instance, barcode);
        });
        return false;
    }

    private static void SpawnInSlotAsyncOnLevelLoad(InventorySlotReceiver slot, Barcode barcode)
    {
        if (!InventorySlotReceiverExtender.Cache.TryGet(slot, out var slotEntity))
        {
            return;
        }

        if (!slotEntity.IsOwner)
        {
            return;
        }

        try
        {
            NetworkSpawnInSlotAsync(slot, barcode);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing NetworkSpawnInSlotAsync", e);
        }
    }

    private static void NetworkSpawnInSlotAsync(InventorySlotReceiver slot, Barcode barcode)
    {
        var spawnable = new Spawnable()
        {
            crateRef = new(barcode),
            policyData = null,
        };

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = slot.transform.position,
            Rotation = slot.transform.rotation,
            SpawnEffect = false,
            SpawnCallback = (info) =>
            {
                var weaponSlotExtender = info.Entity.GetExtender<WeaponSlotExtender>();

                if (weaponSlotExtender == null)
                {
                    return;
                }

                var weaponSlot = weaponSlotExtender.Component;

                if (weaponSlot == null || weaponSlot.interactableHost == null)
                {
                    return;
                }

                slot.OnHandDrop(weaponSlot.interactableHost.TryCast<IGrippable>());
            },
        });
    }
}