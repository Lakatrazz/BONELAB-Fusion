using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class BodyLogEnableData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public bool isEnabled;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(isEnabled);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            isEnabled = reader.ReadBoolean(); ;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static BodyLogEnableData Create(byte smallId, bool isEnabled)
        {
            return new BodyLogEnableData()
            {
                smallId = smallId,
                isEnabled = isEnabled,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class BodyLogEnabledMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BodyLogEnable;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<BodyLogEnableData>())
                {
                    // Enable or disable the body log
                    if (PlayerRep.Representations.TryGetValue(data.smallId, out var rep)) {
                        rep.SetPullCordActive(data.isEnabled);
                    }

                    // Bounce the message back
                    if (NetworkInfo.IsServer)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
