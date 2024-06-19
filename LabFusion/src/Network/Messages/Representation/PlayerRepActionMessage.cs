using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerRepActionData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 3;

    public byte smallId;
    public PlayerActionType type;
    public byte? otherPlayer;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write((byte)type);
        writer.Write(otherPlayer);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        type = (PlayerActionType)reader.ReadByte();
        otherPlayer = reader.ReadByteNullable();
    }

    public static PlayerRepActionData Create(byte smallId, PlayerActionType type, byte? otherPlayer = null)
    {
        return new PlayerRepActionData
        {
            smallId = smallId,
            type = type,
            otherPlayer = otherPlayer,
        };
    }
}

public class PlayerRepActionMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PlayerRepAction;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepActionData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            return;
        }

        PlayerId otherPlayer = data.otherPlayer.HasValue ? PlayerIdManager.GetPlayerId(data.otherPlayer.Value) : null;

        // If this isn't our rig, call these functions
        if (!player.NetworkEntity.IsOwner && player.HasRig)
        {
            var rm = player.RigReferences.RigManager;

            switch (data.type)
            {
                default:
                case PlayerActionType.UNKNOWN:
                    break;
                case PlayerActionType.JUMP:
                    rm.remapHeptaRig.Jump();
                    break;
                case PlayerActionType.DEATH:
                    rm.physicsRig.headSfx.DeathVocal();
                    player.RigReferences.DisableInteraction();
                    break;
                case PlayerActionType.DYING:
                    rm.physicsRig.headSfx.DyingVocal();
                    break;
                case PlayerActionType.RECOVERY:
                    rm.physicsRig.headSfx.RecoveryVocal();
                    break;
                case PlayerActionType.RESPAWN:
                    rm.health.Respawn();
                    rm.physicsRig.TeleportToPose();
                    break;
            }
        }

        // Inform the hooks
        MultiplayerHooking.Internal_OnPlayerAction(player.PlayerId, data.type, otherPlayer);
    }
}
