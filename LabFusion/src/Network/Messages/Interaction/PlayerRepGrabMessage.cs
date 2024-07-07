using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Interaction;

namespace LabFusion.Network;

public class PlayerRepGrabData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 3;

    public byte smallId;
    public Handedness handedness;
    public GrabGroup group;
    public SerializedGrab serializedGrab;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);

        writer.Write((byte)handedness);
        writer.Write((byte)group);

        writer.Write(serializedGrab);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();

        handedness = (Handedness)reader.ReadByte();
        group = (GrabGroup)reader.ReadByte();

        GrabGroupHandler.ReadGrab(ref serializedGrab, reader, group);
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
public class PlayerRepGrabMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepGrab;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepGrabData>();

        // Make sure this isn't us
        if (data.smallId == PlayerIdManager.LocalSmallId)
        {
            return;
        }

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
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