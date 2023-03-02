using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using SLZ.Interaction;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(SimpleGripEvents))]
    public static class SimpleGripEventsPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
        public static bool OnAttachedUpdateDelegatePrefix(SimpleGripEvents __instance, Hand hand)
        {
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager)) {
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
        public static void OnAttachedUpdateDelegatePostfix(SimpleGripEvents __instance, Hand hand) {
            if (NetworkInfo.HasServer && hand.manager == RigData.RigReferences.RigManager && SimpleGripEventsExtender.Cache.TryGet(__instance, out var syncable) && syncable.TryGetExtender<SimpleGripEventsExtender>(out var extender)) {
                if (hand._indexButtonDown) {
                    SendGripEvent(syncable.Id, extender.GetIndex(__instance).Value, SimpleGripEventType.TRIGGER_DOWN);
                }

                if (hand.Controller.GetMenuTap()) {
                    SendGripEvent(syncable.Id, extender.GetIndex(__instance).Value, SimpleGripEventType.MENU_TAP);
                }
            }
        }

        private static void SendGripEvent(ushort syncId, byte gripEventIndex, SimpleGripEventType type) {
            using (var writer = FusionWriter.Create(SimpleGripEventData.Size))
            {
                using (var data = SimpleGripEventData.Create(PlayerIdManager.LocalSmallId, syncId, gripEventIndex, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SimpleGripEvent, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
