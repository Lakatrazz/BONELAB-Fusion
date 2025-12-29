using LabFusion.Data;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class PlayerRepGrabData : INetSerializable
{
    public Handedness Handedness;
    public GrabGroup Group;
    public SerializedGrab SerializedGrab;

    public int? GetSize() => sizeof(byte) * 2 + SerializedGrab.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
        serializer.SerializeValue(ref Group, Precision.OneByte);

        GrabGroupHandler.SerializeGrab(ref SerializedGrab, serializer, Group);
    }

    public Grip GetGrip()
    {
        return SerializedGrab.GetGrip();
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepGrabMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepGrab;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepGrabData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        // Apply grab
        ApplyGrip(sender.Value, data);
    }

    private static void ApplyGrip(byte sender, PlayerRepGrabData data)
    {
        if (!NetworkPlayerManager.TryGetPlayer(sender, out var player))
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

        data.SerializedGrab.RequestGrab(player, data.Handedness, grip);
    }
}