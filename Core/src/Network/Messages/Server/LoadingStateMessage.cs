using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class LoadingStateData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public bool isLoading;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(isLoading);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            isLoading = reader.ReadBoolean();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static LoadingStateData Create(byte smallId, bool isLoading)
        {
            return new LoadingStateData()
            {
                smallId = smallId,
                isLoading = isLoading,
            };
        }
    }

    public class LoadingStateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.LoadingState;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<LoadingStateData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                    else
                    {
                        var playerId = PlayerIdManager.GetPlayerId(data.smallId);
                        playerId.SetLoading(data.isLoading);
                    }
                }
            }
        }
    }
}
