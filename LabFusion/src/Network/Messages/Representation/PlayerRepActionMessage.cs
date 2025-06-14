using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerRepActionData : INetSerializable
{
    public const int Size = sizeof(byte) * 3;

    public byte smallId;
    public PlayerActionType type;
    public byte? otherPlayer;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref otherPlayer);
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

public class PlayerRepActionMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAction;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepActionData>();

        if (!NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            return;
        }

        PlayerID otherPlayer = data.otherPlayer.HasValue ? PlayerIDManager.GetPlayerID(data.otherPlayer.Value) : null;

        // If this isn't our rig, call these functions
        if (!player.NetworkEntity.IsOwner && player.HasRig)
        {
            var rm = player.RigRefs.RigManager;

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
        MultiplayerHooking.InvokeOnPlayerAction(player.PlayerID, data.type, otherPlayer);
    }
}
