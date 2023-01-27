using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Senders;
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
    public class PlayerRepActionData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public PlayerActionType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (PlayerActionType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepActionData Create(byte smallId, PlayerActionType type)
        {
            return new PlayerRepActionData
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    public class PlayerRepActionMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepAction;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PlayerRepActionData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                        var rm = rep.RigReferences.RigManager;

                        // Make sure the rig exists
                        if (!rm.IsNOC()) {
                            switch (data.type) {
                                default:
                                case PlayerActionType.UNKNOWN:
                                    break;
                                case PlayerActionType.JUMP:
                                    rm.openControllerRig.Jump();
                                    break;
                                case PlayerActionType.DEATH:
                                    rm.physicsRig.headSfx.DeathVocal();
                                    rep.DetachRepGrips();
                                    break;
                                case PlayerActionType.DYING:
                                    rm.physicsRig.headSfx.DyingVocal();
                                    break;
                                case PlayerActionType.RECOVERY:
                                    rm.physicsRig.headSfx.RecoveryVocal();
                                    break;
                            }
                        }

                        // Inform the hooks
                        MultiplayerHooking.Internal_OnPlayerAction(rep.PlayerId, data.type);
                    }
                }
            }
        }
    }
}
