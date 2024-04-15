using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Syncables;
using LabFusion.Patching;

namespace LabFusion.Network
{
    [Net.DelayWhileTargetLoading]
    public class PropHealthDestroyMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropHealthDestroy;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<ComponentIndexData>();

            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                if (SyncManager.TryGetSyncable<PropSyncable>(data.syncId, out var health) && health.TryGetExtender<PropHealthExtender>(out var extender))
                {
                    var propHealth = extender.GetComponent(data.componentIndex);
                    PropHealthPatches.IgnorePatches = true;

                    propHealth.hits = propHealth.req_hit_count + 1;
                    propHealth.bloodied = true;

                    try
                    {
                        propHealth.TIMEDKILL();
                        propHealth.SETPROP();
                    }
                    catch
                    {
#if DEBUG
                        FusionLogger.Warn("Got error trying to destroy a PropHealth. This is probably caused by the item.");
#endif
                    }

                    PropHealthPatches.IgnorePatches = false;
                }
            }
        }
    }
}
