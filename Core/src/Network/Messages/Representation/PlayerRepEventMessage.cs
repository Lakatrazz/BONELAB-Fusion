using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public enum PlayerRepEventType {
        UNKNOWN = 0,
        JUMP = 1,
    }

    public class PlayerRepEventData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public PlayerRepEventType type;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (PlayerRepEventType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepEventData Create(byte smallId, PlayerRepEventType type)
        {
            return new PlayerRepEventData
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    public class PlayerRepEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PlayerRepEventData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else if (PlayerRep.Representations.TryGetValue(data.smallId, out var rep)) {
                        switch (data.type) {
                            default:
                            case PlayerRepEventType.UNKNOWN:
                                break;
                            case PlayerRepEventType.JUMP:
                                if (!rep.RigReferences.RigManager.IsNOC())
                                    rep.RigReferences.RigManager.openControllerRig.Jump();
                                break;
                        }
                    }
                }
            }
        }
    }
}
