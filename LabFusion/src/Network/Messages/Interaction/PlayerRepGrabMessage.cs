using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerRepGrabData : INetSerializable
{
    public const int Size = sizeof(byte) * 3;

    public byte smallId;
    public Handedness handedness;
    public GrabGroup group;
    public SerializedGrab serializedGrab;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref handedness, Precision.OneByte);
        serializer.SerializeValue(ref group, Precision.OneByte);

        GrabGroupHandler.SerializeGrab(ref serializedGrab, serializer, group);
    }

    public Grip GetGrip()
    {
        return serializedGrab.GetGrip();
    }

    public NetworkPlayer GetPlayer()
    {
        if (NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            return player;
        }

        return null;
    }

    public static PlayerRepGrabData Create(byte smallId, Handedness handedness, GrabGroup group, SerializedGrab serializedGrab)
    {
        return new PlayerRepGrabData()
        {
            smallId = smallId,
            handedness = handedness,
            group = group,
            serializedGrab = serializedGrab
        };
    }
}

[Net.DelayWhileTargetLoading]
public class PlayerRepGrabMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepGrab;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepGrabData>();

        // Make sure this isn't us
        if (data.smallId == PlayerIDManager.LocalSmallID)
        {
            return;
        }

        // Apply grab
        ApplyGrip(data);
    }

    private static void ApplyGrip(PlayerRepGrabData data)
    {
        var player = data.GetPlayer();

        if (player == null)
        {
#if DEBUG
            FusionLogger.Warn("Grab message requested a player to grab that doesn't exist?");
#endif
            return;
        }

        var grip = data.GetGrip();

        if (grip == null)
        {
            return;
        }

        data.serializedGrab.RequestGrab(player, data.handedness, grip);
    }
}