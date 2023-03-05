using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class SlowMoButtonMessageData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2;

        public byte smallId;
        public bool isDecrease;

        public static SlowMoButtonMessageData Create(byte smallId, bool isDecrease) {
            return new SlowMoButtonMessageData() {
                smallId = smallId,
                isDecrease = isDecrease
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(isDecrease);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            isDecrease = reader.ReadBoolean();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }

    [Net.SkipHandleWhileLoading]
    public class SlowMoButtonMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SlowMoButton;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<SlowMoButtonMessageData>()) {
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        Control_GlobalTimePatches.IgnorePatches = true;

                        if (RigData.HasPlayer) {
                            var rm = RigData.RigReferences.RigManager;
                            var controlTime = rm.openControllerRig.globalTimeControl;

                            if (controlTime != null) {
                                if (data.isDecrease)
                                    controlTime.DECREASE_TIMESCALE();
                                else
                                    controlTime.TOGGLE_TIMESCALE();
                            }
                        }

                        Control_GlobalTimePatches.IgnorePatches = false;
                    }
                }
            }
        }
    }
}
