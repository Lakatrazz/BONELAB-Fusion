using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Messages;

public class BodyLogToggleData : INetSerializable
{
    public const int Size = sizeof(byte) * 2;

    public byte PlayerID;
    public bool IsEnabled;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref IsEnabled);
    }
}

[Net.SkipHandleWhileLoading]
public class BodyLogToggleMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BodyLogToggleData>();

        // Set the enabled state of the body log
        if (NetworkPlayerManager.TryGetPlayer(data.PlayerID, out var player) && !player.NetworkEntity.IsOwner)
        {
            SetBallEnabled(player, data.IsEnabled);
        }
    }

    private static void SetBallEnabled(NetworkPlayer player, bool isEnabled)
    {
        if (!player.HasRig)
        {
            return;
        }

        var pullCord = player.RigRefs.RigManager.GetComponentInChildren<PullCordDevice>(true);

        if (pullCord == null)
        {
            return;
        }

        // If the ball should be enabled, make the distance required infinity so it always shows
        if (isEnabled)
        {
            pullCord.handShowDist = float.PositiveInfinity;
        }
        // If it should be disabled, make the distance zero so that it disables itself
        else
        {
            pullCord.handShowDist = 0f;
        }
    }
}