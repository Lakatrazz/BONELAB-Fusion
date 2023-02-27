using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class BodyLogEffectData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte);

        public byte smallId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static BodyLogEffectData Create(byte smallId)
        {
            return new BodyLogEffectData()
            {
                smallId = smallId,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class BodyLogEffectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BodyLogEffect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<BodyLogEffectData>())
                {
                    // Play the effect
                    if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                        rep.PlayPullCordEffects();
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
