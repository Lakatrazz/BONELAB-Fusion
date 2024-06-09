using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Network
{
    public class GamemodeMetadataSetData : IFusionSerializable
    {
        public ushort gamemodeId;
        public string key;
        public string value;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(gamemodeId);
            writer.Write(key);
            writer.Write(value);
        }

        public void Deserialize(FusionReader reader)
        {
            gamemodeId = reader.ReadUInt16();
            key = reader.ReadString();
            value = reader.ReadString();
        }

        public static GamemodeMetadataSetData Create(ushort gamemodeId, string key, string value)
        {
            return new GamemodeMetadataSetData()
            {
                gamemodeId = gamemodeId,
                key = key,
                value = value,
            };
        }
    }

    public class GamemodeMetadataSetMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GamemodeMetadataSet;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsClient || !isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<GamemodeMetadataSetData>();
                if (GamemodeManager.TryGetGamemode(data.gamemodeId, out var gamemode))
                {
                    gamemode.Metadata.ForceSetLocalMetadata(data.key, data.value);
                }
            }
            else
                throw new ExpectedClientException();
        }
    }
}
