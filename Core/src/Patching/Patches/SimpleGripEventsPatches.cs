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
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SimpleGripEvents.OnAttachedUpdateDelegate))]
        public static void OnAttachedUpdateDelegate(SimpleGripEvents __instance, Hand hand) {
            if (NetworkInfo.HasServer && hand.manager == RigData.RigReferences.RigManager && PropSyncable.SimpleGripEventsCache.TryGet(__instance, out var syncable)) {
                if (hand._indexButtonDown) {
                    SendGripEvent(syncable.Id, syncable.GetIndex(__instance).Value, SimpleGripEventType.TRIGGER_DOWN);
                }

                if (hand.Controller.GetMenuTap()) {
                    SendGripEvent(syncable.Id, syncable.GetIndex(__instance).Value, SimpleGripEventType.MENU_TAP);
                }
            }
        }

        private static void SendGripEvent(ushort syncId, byte gripEventIndex, SimpleGripEventType type) {
            using (var writer = FusionWriter.Create())
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
