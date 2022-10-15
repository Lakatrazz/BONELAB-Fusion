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
    public class PlayerRepAnchorData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public SerializedGripAnchor serializedGripAnchor;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(serializedGripAnchor);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            serializedGripAnchor = reader.ReadFusionSerializable<SerializedGripAnchor>();
        }

        public PlayerRep GetRep()
        {
            if (PlayerRep.Representations.ContainsKey(smallId))
                return PlayerRep.Representations[smallId];
            return null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepAnchorData Create(byte smallId, SerializedGripAnchor serializedGripAnchor)
        {
            return new PlayerRepAnchorData()
            {
                smallId = smallId,
                serializedGripAnchor = serializedGripAnchor,
            };
        }
    }

    public class PlayerRepAnchorMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepAnchors;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepAnchorData>()) {
                    if (data.smallId != PlayerIdManager.LocalSmallId) {
                        var rep = data.GetRep();

                        if (rep != null) {
                            var hand = rep.RigReferences.GetHand(data.serializedGripAnchor.handedness);
                            if (hand.m_CurrentAttachedGO)
                                rep.RigReferences.SetSerializedAnchor(data.serializedGripAnchor.handedness, data.serializedGripAnchor);
                        }

                        // Send message to other clients if server
                        if (NetworkInfo.IsServer && isServerHandled) {
                            using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
                            }
                        }
                    }
                }
            }
        }
    }
}
