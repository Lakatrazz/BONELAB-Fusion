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
    public class GamemodeMetadataResponseData : IFusionSerializable, IDisposable {
        public ushort gamemodeId;
        public string key;
        public string value;

        public void Serialize(FusionWriter writer) {
            writer.Write(gamemodeId);
            writer.Write(key);
            writer.Write(value);
        }
        
        public void Deserialize(FusionReader reader) {
            gamemodeId = reader.ReadUInt16();
            key = reader.ReadString();
            value = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static GamemodeMetadataResponseData Create(ushort gamemodeId, string key, string value) {
            return new GamemodeMetadataResponseData() {
                gamemodeId = gamemodeId,
                key = key,
                value = value,
            };
        }

        public static GamemodeMetadataResponseData Create(Gamemode gamemode, string key, string value)
        {
            return new GamemodeMetadataResponseData()
            {
                gamemodeId = gamemode.Tag.HasValue ? gamemode.Tag.Value : ushort.MaxValue,
                key = key,
                value = value,
            };
        }
    }

    public class GamemodeMetadataResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GamemodeMetadataResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<GamemodeMetadataResponseData>())
                    {
                        if (GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode)) {
                            gamemode.Internal_ForceSetMetadata(data.key, data.value);
                        }
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
