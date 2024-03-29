using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ;

using System;

namespace LabFusion.Network
{
    public class SpawnRequestData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
                using var data = reader.ReadFusionSerializable<SpawnRequestData>();
                var playerId = PlayerIdManager.GetPlayerId(data.owner);

                // Check if we should ignore the spawn gun request
                if (playerId != null && !playerId.IsSelf && FusionDevTools.PreventSpawnGun(playerId))
                {
                    return;
                }

                var syncId = SyncManager.AllocateSyncID();

                PooleeUtilities.SendSpawn(data.owner, data.barcode, syncId, data.serializedTransform, false, null, data.trackerId);
            }
            else
                throw new ExpectedServerException();
        }
    }
}
