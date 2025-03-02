using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum TimeTrialGameControllerType
{
    UNKNOWN = 0,
    UpdateDifficulty = 1,
    TIMETRIAL_PlayerStartTrigger = 2,
    TIMETRIAL_PlayerEndTrigger = 3,
    ProgPointKillCount = 4,
    SetRequiredKillCount = 5,
}

public class TimeTrialGameControllerData : INetSerializable
{
    public TimeTrialGameControllerType type;
    public byte value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref value);
    }

    public static TimeTrialGameControllerData Create(TimeTrialGameControllerType type, int value)
    {
        return new TimeTrialGameControllerData()
        {
            type = type,
            value = (byte)value,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class TimeTrialGameControllerMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.TimeTrial_GameController;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<TimeTrialGameControllerData>();

        if (!TimeTrialData.IsInTimeTrial)
        {
            return;
        }

        TimeTrial_GameControllerPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case TimeTrialGameControllerType.UNKNOWN:
                break;
            case TimeTrialGameControllerType.UpdateDifficulty:
                TimeTrialData.GameController.UpdateDifficulty(data.value);
                break;
            case TimeTrialGameControllerType.TIMETRIAL_PlayerStartTrigger:
                TimeTrialData.GameController.TIMETRIAL_PlayerStartTrigger();
                break;
            case TimeTrialGameControllerType.TIMETRIAL_PlayerEndTrigger:
                TimeTrialData.GameController.TIMETRIAL_PlayerEndTrigger();
                break;
            case TimeTrialGameControllerType.ProgPointKillCount:
                TimeTrialData.GameController.ProgPointKillCount(data.value);
                break;
            case TimeTrialGameControllerType.SetRequiredKillCount:
                TimeTrialData.GameController.SetRequiredKillCount(data.value);
                break;
        }

        TimeTrial_GameControllerPatches.IgnorePatches = false;
    }
}
