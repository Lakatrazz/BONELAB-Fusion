using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Bonelab.Extenders;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(SimpleGripEvents))]
public static class SimpleGripEventsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedDelegate))]
    public static bool OnAttachedDelegatePrefix(SimpleGripEvents __instance, Hand hand)
    {
        if (IsPlayerRep(__instance, hand))
        {
            return false;
        }
        else if (GetExtender(__instance, hand, out var syncable, out var extender))
        {
            // Decompiled code from CPP2IL
            if (__instance.doNotRetriggerOnMultiGirp)
            {
                if (!__instance.leftHand && !__instance.rightHand)
                {
                    SendGripEvent(syncable.Id, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
                }
            }
            else
            {
                SendGripEvent(syncable.Id, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnDetachedDelegate))]
    public static bool OnDetachedDelegatePrefix(SimpleGripEvents __instance, Hand hand)
    {
        if (IsPlayerRep(__instance, hand))
        {
            return false;
        }
        else if (GetExtender(__instance, hand, out var syncable, out var extender))
        {
            // Decompiled code from CPP2IL
            if (__instance.doNotRetriggerOnMultiGirp)
            {
                bool rightHand = __instance.rightHand;
                bool leftHand = __instance.leftHand;

                if (hand.handedness != Handedness.LEFT)
                {
                    rightHand = false;
                }
                leftHand = false; // This probably isn't how the logic is supposed to be but it's what the game does /shrug
                if (leftHand || rightHand)
                {
                    return true;
                }
            }
            SendGripEvent(syncable.Id, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.DETACH);
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
    public static bool OnAttachedUpdateDelegatePrefix(SimpleGripEvents __instance, Hand hand)
    {
        return !IsPlayerRep(__instance, hand);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
    public static void OnAttachedUpdateDelegatePostfix(SimpleGripEvents __instance, Hand hand)
    {
        if (GetExtender(__instance, hand, out var syncable, out var extender))
        {
            if (hand._indexButtonDown)
            {
                SendGripEvent(syncable.Id, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.TRIGGER_DOWN);
            }

            if (hand.Controller.GetMenuTap())
            {
                SendGripEvent(syncable.Id, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.MENU_TAP);
            }
        }
    }

    private static bool IsPlayerRep(SimpleGripEvents __instance, Hand hand)
    {
        if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager) && SimpleGripEventsExtender.Cache.ContainsSource(__instance))
        {
            return true;
        }

        return false;
    }

    private static bool GetExtender(SimpleGripEvents __instance, Hand hand, out NetworkEntity entity, out SimpleGripEventsExtender extender)
    {
        entity = null;
        extender = null;

        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        if (!hand.manager.IsSelf())
        {
            return false;
        }

        if (!SimpleGripEventsExtender.Cache.TryGet(__instance, out entity))
        {
            return false;
        }

        extender = entity.GetExtender<SimpleGripEventsExtender>();

        return true;
    }

    private static void SendGripEvent(ushort syncId, byte gripEventIndex, SimpleGripEventType type)
    {
        using var writer = FusionWriter.Create(SimpleGripEventData.Size);
        var data = SimpleGripEventData.Create(PlayerIdManager.LocalSmallId, syncId, gripEventIndex, type);
        writer.Write(data);

        using var message = FusionMessage.ModuleCreate<SimpleGripEventMessage>(writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}