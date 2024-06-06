using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network
{
    public class GamemodeMetadataRemoveData : IFusionSerializable
    {
        public ushort gamemodeId;
        public string key;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(gamemodeId);
            writer.Write(key);
        }

        public void Deserialize(FusionReader reader)
        {
            gamemodeId = reader.ReadUInt16();
            key = reader.ReadString();
        }

        public static GamemodeMetadataRemoveData Create(ushort gamemodeId, string key)
        {
            return new GamemodeMetadataRemoveData()
            {
                gamemodeId = gamemodeId,
                key = key,
            };
        }
    }

    public class GamemodeMetadataRemoveMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GamemodeMetadataRemove;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<GamemodeMetadataRemoveData>();
                if (GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode))
                {
                    gamemode.Metadata.ForceRemoveLocalMetadata(data.key);
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
