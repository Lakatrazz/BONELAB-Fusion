using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerRepActionData : INetSerializable
{
    public const int Size = sizeof(byte) * 2;

    public PlayerActionType Type;

    public byte? OtherPlayer;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref OtherPlayer);
    }
}

public class PlayerRepActionMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAction;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepActionData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            return;
        }

        PlayerID otherPlayer = data.OtherPlayer.HasValue ? PlayerIDManager.GetPlayerID(data.OtherPlayer.Value) : null;

        // If this isn't our rig, call these functions
        if (!player.NetworkEntity.IsOwner && player.HasRig)
        {
            var rm = player.RigRefs.RigManager;

            switch (data.Type)
            {
                default:
                case PlayerActionType.UNKNOWN:
                    break;
                case PlayerActionType.JUMP:
                    rm.remapHeptaRig.Jump();
                    break;
                case PlayerActionType.DEATH:
                    rm.physicsRig.headSfx.DeathVocal();
                    player.RigRefs.DisableInteraction();
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
        MultiplayerHooking.InvokeOnPlayerAction(player.PlayerID, data.Type, otherPlayer);
    }
}
