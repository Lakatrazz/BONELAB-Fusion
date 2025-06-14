using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

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
    public TimeTrialGameControllerType Type;
    public byte Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref Value);
    }
}

[Net.DelayWhileTargetLoading]
public class TimeTrialGameControllerMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<TimeTrialGameControllerData>();

        if (!TimeTrialEventHandler.IsInTimeTrial)
        {
            return;
        }

        TimeTrial_GameControllerPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case TimeTrialGameControllerType.UNKNOWN:
                    break;
                case TimeTrialGameControllerType.UpdateDifficulty:
                    TimeTrialEventHandler.GameController.UpdateDifficulty(data.Value);
                    break;
                case TimeTrialGameControllerType.TIMETRIAL_PlayerStartTrigger:
                    TimeTrialEventHandler.GameController.TIMETRIAL_PlayerStartTrigger();
                    break;
                case TimeTrialGameControllerType.TIMETRIAL_PlayerEndTrigger:
                    TimeTrialEventHandler.GameController.TIMETRIAL_PlayerEndTrigger();
                    break;
                case TimeTrialGameControllerType.ProgPointKillCount:
                    TimeTrialEventHandler.GameController.ProgPointKillCount(data.Value);
                    break;
                case TimeTrialGameControllerType.SetRequiredKillCount:
                    TimeTrialEventHandler.GameController.SetRequiredKillCount(data.Value);
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling TimeTrialGameControllerMessage", e);
        }

        TimeTrial_GameControllerPatches.IgnorePatches = false;
    }
}
