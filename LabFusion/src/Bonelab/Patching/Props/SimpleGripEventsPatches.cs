using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Bonelab.Extenders;
using LabFusion.Marrow.Integration;
using LabFusion.Scene;

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
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (IsPlayerRep(__instance, hand))
        {
            return false;
        }

        if (GetExtender(__instance, hand, out var entity, out var extender))
        {
            // Decompiled code from CPP2IL
            if (__instance.doNotRetriggerOnMultiGirp)
            {
                if (!__instance.leftHand && !__instance.rightHand)
                {
                    SendGripEvent(entity.ID, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
                }
            }
            else
            {
                SendGripEvent(entity.ID, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.ATTACH);
            }

            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnDetachedDelegate))]
    public static bool OnDetachedDelegatePrefix(SimpleGripEvents __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (IsPlayerRep(__instance, hand))
        {
            return false;
        }

        if (GetExtender(__instance, hand, out var entity, out var extender))
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

            SendGripEvent(entity.ID, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.DETACH);

            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
    public static bool OnAttachedUpdateDelegatePrefix(SimpleGripEvents __instance, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (IsPlayerRep(__instance, hand))
        {
            return false;
        }

        if (GetExtender(__instance, hand, out var entity, out var extender))
        {
            if (hand._indexButtonDown)
            {
                SendGripEvent(entity.ID, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.TRIGGER_DOWN);
            }

            if (hand.Controller.GetMenuTap())
            {
                SendGripEvent(entity.ID, (byte)extender.GetIndex(__instance).Value, SimpleGripEventType.MENU_TAP);
            }

            return false;
        }

        return true;
    }

    private static bool IsPlayerRep(SimpleGripEvents __instance, Hand hand)
    {
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        if (NetworkPlayerManager.HasExternalPlayer(hand.manager) && SimpleGripEventsExtender.Cache.ContainsSource(__instance))
        {
            return true;
        }

        return false;
    }

    private static bool GetExtender(SimpleGripEvents __instance, Hand hand, out NetworkEntity entity, out SimpleGripEventsExtender extender)
    {
        entity = null;
        extender = null;

        // Make sure there is a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Make sure the grabbing hand is the local player
        if (!hand.manager.IsLocalPlayer())
        {
            return false;
        }

        // Get the extender
        if (!SimpleGripEventsExtender.Cache.TryGet(__instance, out entity))
        {
            return false;
        }

        extender = entity.GetExtender<SimpleGripEventsExtender>();

        // Check for desync script
        if (Desyncer.Cache.ContainsSource(__instance.gameObject))
        {
            return false;
        }

        return true;
    }

    private static void SendGripEvent(ushort entityId, byte gripEventIndex, SimpleGripEventType type)
    {
        var data = new SimpleGripEventData()
        {
            Entity = new NetworkEntityReference(entityId),
            GripEventIndex = gripEventIndex,
            Type = type,
        };

        MessageRelay.RelayModule<SimpleGripEventMessage, SimpleGripEventData>(data, CommonMessageRoutes.ReliableToClients);
    }
}