using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Bonelab.Patching;

namespace LabFusion.Bonelab.Messages;

public enum KartRaceEventType
{
    UNKNOWN = 0,
    START_RACE = 1,
    NEW_LAP = 2,
    RESET_RACE = 3,
    END_RACE = 4,
}

public class KartRaceEventData : INetSerializable
{
    public KartRaceEventType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class KartRaceEventMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<KartRaceEventData>();

        KartRacePatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case KartRaceEventType.UNKNOWN:
                    break;
                case KartRaceEventType.START_RACE:
                    MonogonMotorwayEventHandler.GameController.STARTRACE();
                    break;
                case KartRaceEventType.NEW_LAP:
                    MonogonMotorwayEventHandler.GameController.NEWLAP();
                    break;
                case KartRaceEventType.RESET_RACE:
                    MonogonMotorwayEventHandler.GameController.RESETRACE();
                    break;
                case KartRaceEventType.END_RACE:
                    MonogonMotorwayEventHandler.GameController.ENDRACE();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling KartRaceEventMessage", e);
        }

        KartRacePatches.IgnorePatches = false;
    }
}
