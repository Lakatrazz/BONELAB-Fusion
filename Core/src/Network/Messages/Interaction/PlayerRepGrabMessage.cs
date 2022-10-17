using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;

namespace LabFusion.Network
{
    public class PlayerRepGrabData : IFusionSerializable, IDisposable {
        public byte smallId;
        public Handedness handedness;
        public SyncUtilities.SyncGroup group;
        public SerializedGrab serializedGrab;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write((byte)handedness);
            writer.Write((byte)group);
            writer.Write(serializedGrab);
        }

        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            handedness = (Handedness)reader.ReadByte();
            group = (SyncUtilities.SyncGroup)reader.ReadByte();
            
            if (SyncUtilities.SerializedGrabTypes.ContainsKey(group)) {
                var type = SyncUtilities.SerializedGrabTypes[group];
                serializedGrab = (SerializedGrab)reader.ReadFusionSerializable(type);
            }
        }

        public Grip GetGrip() {
            return serializedGrab.GetGrip();
        }

        public PlayerRep GetRep() {
            if (PlayerRep.Representations.ContainsKey(smallId))
                return PlayerRep.Representations[smallId];
            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepGrabData Create(byte smallId, Handedness handedness, SyncUtilities.SyncGroup group, SerializedGrab serializedGrab) {
            return new PlayerRepGrabData() {
                smallId = smallId,
                handedness = handedness,
                group = group,
                serializedGrab = serializedGrab
            };
        }
    }

    public class PlayerRepGrabMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepGrab;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepGrabData>()) {

                    if (data.smallId != PlayerIdManager.LocalSmallId) {
                        var rep = data.GetRep();
                        var grip = data.GetGrip();

                        if (rep != null && grip != null) {
                            data.serializedGrab.RequestGrab(rep, data.handedness, grip);
                        }
                        else {
#if DEBUG
                            FusionLogger.Warn($"Failed to execute Player Grab message! Rep was {rep != null}, grip was {grip != null}");
#endif
                        }

                        // Send message to other clients if server
                        if (NetworkInfo.IsServer && isServerHandled) {
                            using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }
    }
}
