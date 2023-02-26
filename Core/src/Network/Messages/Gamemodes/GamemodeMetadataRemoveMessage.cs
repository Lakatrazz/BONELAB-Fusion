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
    public class GamemodeMetadataRemoveData : IFusionSerializable, IDisposable {
        public ushort gamemodeId;
        public string key;

        public void Serialize(FusionWriter writer) {
            writer.Write(gamemodeId);
            writer.Write(key);
        }
        
        public void Deserialize(FusionReader reader) {
            gamemodeId = reader.ReadUInt16();
            key = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static GamemodeMetadataRemoveData Create(ushort gamemodeId, string key) {
            return new GamemodeMetadataRemoveData() {
                gamemodeId = gamemodeId,
                key = key,
            };
        }
    }

    public class GamemodeMetadataRemoveMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GamemodeMetadataRemove;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<GamemodeMetadataRemoveData>())
                    {
                        if (GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode)) {
                            gamemode.Internal_ForceRemoveMetadata(data.key);
                        }
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
