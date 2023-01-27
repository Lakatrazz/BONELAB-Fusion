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
    public class PlayerSettingsData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public SerializedPlayerSettings settings;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(settings);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            settings = reader.ReadFusionSerializable<SerializedPlayerSettings>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerSettingsData Create(byte smallId, SerializedPlayerSettings settings)
        {
            return new PlayerSettingsData()
            {
                smallId = smallId,
                settings = settings,
            };
        }
    }

    public class PlayerSettingsMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerSettings;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PlayerSettingsData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                            rep.SetSettings(data.settings);
                        }
                    }
                }
            }
        }
    }
}
