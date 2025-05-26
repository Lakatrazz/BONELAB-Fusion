using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Bonelab.Patching;

namespace LabFusion.Bonelab.Messages;

public enum DescentElevatorType
{
    UNKNOWN = 0,
    START_ELEVATOR = 1,
    STOP_ELEVATOR = 2,
    SEAL_DOORS = 3,
    START_MOVE_UPWARD = 4,
    SLOW_UPWARD_MOVEMENT = 5,
    OPEN_DOORS = 6,
    CLOSE_DOORS = 7,
}

public class DescentElevatorData : INetSerializable
{
    public DescentElevatorType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class DescentElevatorMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DescentElevatorData>();

        if (!DescentData.Elevator)
        {
            DescentData.Instance.CacheValues();
        }

        TutorialElevatorPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case DescentElevatorType.UNKNOWN:
                    break;
                case DescentElevatorType.START_ELEVATOR:
                    DescentData.Elevator.StartElevator();
                    break;
                case DescentElevatorType.STOP_ELEVATOR:
                    DescentData.Elevator.StopDoorRoutine();
                    break;
                case DescentElevatorType.SEAL_DOORS:
                    DescentData.Elevator.SealDoors();
                    break;
                case DescentElevatorType.START_MOVE_UPWARD:
                    DescentData.Elevator.StartMoveUpward();
                    break;
                case DescentElevatorType.SLOW_UPWARD_MOVEMENT:
                    DescentData.Elevator.SlowUpwardMovement();
                    break;
                case DescentElevatorType.OPEN_DOORS:
                    DescentData.Elevator.OpenDoors();
                    break;
                case DescentElevatorType.CLOSE_DOORS:
                    DescentData.Elevator.CloseDoors();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling DescentElevatorMessage", e);
        }

        TutorialElevatorPatches.IgnorePatches = false;
    }
}