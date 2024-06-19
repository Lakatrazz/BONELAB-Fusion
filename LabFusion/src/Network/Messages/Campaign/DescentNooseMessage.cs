using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network
{
    public enum DescentNooseType
    {
        UNKNOWN = 0,
        ATTACH_NOOSE = 1,
        CUT_NOOSE = 2,
    }

    public class DescentNooseData : IFusionSerializable
    {
        public byte smallId;
        public DescentNooseType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (DescentNooseType)reader.ReadByte();
        }

        public static DescentNooseData Create(byte smallId, DescentNooseType type)
        {
            return new DescentNooseData()
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class DescentNooseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DescentNoose;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<DescentNooseData>();
            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                NoosePatches.IgnorePatches = true;

                // Register a noose event for catchup
                _ = DescentData.CreateNooseEvent(data.smallId, data.type);

                switch (data.type)
                {
                    default:
                    case DescentNooseType.UNKNOWN:
                        break;
                    case DescentNooseType.ATTACH_NOOSE:
                        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var rep))
                        {
                            // Assign the RigManager and Health to the noose
                            // We assign the rigmanager so the noose knows what neck to joint to
                            // The player health is also assigned so it doesn't damage the local player
                            DescentData.Noose.rM = rep.RigReferences.RigManager;
                            DescentData.Noose.pH = rep.RigReferences.Health;

                            // Now we actually attach the neck of the player
                            DescentData.Noose.AttachNeck();
                        }
                        break;
                    case DescentNooseType.CUT_NOOSE:
                        // This function is called to cut the noose as if a knife cut it
                        DescentData.Noose.NooseCut();

                        DescentData.CheckAchievement();
                        break;
                }

                NoosePatches.IgnorePatches = false;
            }
        }
    }
}
