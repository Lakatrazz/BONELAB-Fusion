using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using LabFusion.Syncables;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network
{
    public class MagazineClaimData : IFusionSerializable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte owner;
        public ushort syncId;
        public Handedness handedness;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(syncId);
            writer.Write((byte)handedness);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            syncId = reader.ReadUInt16();
            handedness = (Handedness)reader.ReadByte();
        }

        public static MagazineClaimData Create(byte owner, ushort syncId, Handedness handedness)
        {
            return new MagazineClaimData()
            {
                owner = owner,
                syncId = syncId,
                handedness = handedness,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class MagazineClaimMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MagazineClaim;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<MagazineClaimData>();

            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.owner, NetworkChannel.Reliable, message, false);
            }
            else if (SyncManager.TryGetSyncable<PropSyncable>(data.syncId, out var syncable) && syncable.TryGetExtender<MagazineExtender>(out var extender) && PlayerRepManager.TryGetPlayerRep(data.owner, out var rep))
            {
                MagazineUtilities.GrabMagazine(extender.Component, rep.RigReferences.RigManager, data.handedness);
            }
        }
    }
}
