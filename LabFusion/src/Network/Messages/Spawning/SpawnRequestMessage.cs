using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class SpawnRequestData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2 + SerializedTransform.Size;

        public byte owner;
        public string barcode;
        public SerializedTransform serializedTransform;

        public uint trackerId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(barcode);
            writer.Write(serializedTransform);

            writer.Write(trackerId);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            barcode = reader.ReadString();
            serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();

            trackerId = reader.ReadUInt32();
        }

        public static SpawnRequestData Create(byte owner, string barcode, SerializedTransform serializedTransform, uint trackerId)
        {
            return new SpawnRequestData()
            {
                owner = owner,
                barcode = barcode,
                serializedTransform = serializedTransform,

                trackerId = trackerId,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SpawnRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<SpawnRequestData>();
                var playerId = PlayerIdManager.GetPlayerId(data.owner);

                var syncId = SyncManager.AllocateSyncID();

                PooleeUtilities.SendSpawn(data.owner, data.barcode, syncId, data.serializedTransform, false, data.trackerId);
            }
            else
                throw new ExpectedServerException();
        }
    }
}
