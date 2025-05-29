using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Patching;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum DescentNooseType
{
    UNKNOWN = 0,
    ATTACH_NOOSE = 1,
    CUT_NOOSE = 2,
}

public class DescentNooseData : INetSerializable
{
    public byte PlayerId;
    public DescentNooseType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class DescentNooseMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DescentNooseData>();

        NooseBonelabIntroPatches.IgnorePatches = true;

        // Register a noose event for catchup
        _ = DescentEventHandler.CreateNooseEvent(data.PlayerId, data.Type);

        try
        {
            switch (data.Type)
            {
                default:
                case DescentNooseType.UNKNOWN:
                    break;
                case DescentNooseType.ATTACH_NOOSE:
                    if (NetworkPlayerManager.TryGetPlayer(data.PlayerId, out var player))
                    {
                        // Assign the RigManager and Health to the noose
                        // We assign the rigmanager so the noose knows what neck to joint to
                        // The player health is also assigned so it doesn't damage the local player
                        DescentEventHandler.Noose.rM = player.RigRefs.RigManager;
                        DescentEventHandler.Noose.pH = player.RigRefs.Health;

                        // Now we actually attach the neck of the player
                        DescentEventHandler.Noose.AttachNeck();
                    }
                    break;
                case DescentNooseType.CUT_NOOSE:
                    // This function is called to cut the noose as if a knife cut it
                    DescentEventHandler.Noose.NooseCut();

                    DescentEventHandler.CheckAchievement();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling DescentNooseMessage", e);
        }

        NooseBonelabIntroPatches.IgnorePatches = false;
    }
}