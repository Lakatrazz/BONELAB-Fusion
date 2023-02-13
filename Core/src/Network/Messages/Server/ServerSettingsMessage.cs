using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

namespace LabFusion.Network
{
    public class ServerSettingsData : IFusionSerializable, IDisposable
    {
        public SerializedServerSettings settings;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(settings);
        }

        public void Deserialize(FusionReader reader)
        {
            settings = reader.ReadFusionSerializable<SerializedServerSettings>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ServerSettingsData Create(SerializedServerSettings settings)
        {
            return new ServerSettingsData()
            {
                settings = settings,
            };
        }
    }

    public class ServerSettingsMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ServerSettings;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ServerSettingsData>())
                {
                    // ONLY clients should receive this!
                    if (!NetworkInfo.IsServer) {
                        FusionPreferences.ReceivedServerSettings = data.settings.settings;
                        MultiplayerHooking.Internal_OnServerSettingsChanged();
                    }
                    else
                        throw new ExpectedClientException();
                }
            }
        }
    }
}
