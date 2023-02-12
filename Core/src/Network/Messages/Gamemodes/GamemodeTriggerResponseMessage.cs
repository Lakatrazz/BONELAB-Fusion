using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network
{
    public class GamemodeTriggerResponseData : IFusionSerializable, IDisposable {
        public ushort gamemodeId;
        public string value;

        public void Serialize(FusionWriter writer) {
            writer.Write(gamemodeId);
            writer.Write(value);
        }
        
        public void Deserialize(FusionReader reader) {
            gamemodeId = reader.ReadUInt16();
            value = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static GamemodeTriggerResponseData Create(ushort gamemodeId, string value) {
            return new GamemodeTriggerResponseData() {
                gamemodeId = gamemodeId,
                value = value,
            };
        }

        public static GamemodeTriggerResponseData Create(Gamemode gamemode, string value)
        {
            return new GamemodeTriggerResponseData()
            {
                gamemodeId = gamemode.Tag.HasValue ? gamemode.Tag.Value : ushort.MaxValue,
                value = value,
            };
        }
    }

    public class GamemodeTriggerResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GamemodeTriggerResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<GamemodeTriggerResponseData>())
                    {
                        if (GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode)) {
                            gamemode.Internal_TriggerEvent(data.value);
                        }
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
