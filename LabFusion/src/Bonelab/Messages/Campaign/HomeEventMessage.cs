using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum HomeEventType
{
    UNKNOWN = 0,
    WARMUP_JIMMY_ARM = 1,
    REACH_WINDMILL = 2,
    REACHED_TAXI = 3,
    ARM_HIDE = 4,
    DRIVING_END = 5,
    COMPLETE_GAME = 6,
    SPLINE_LOOP_COUNTER = 7,
}

public class HomeEventData : INetSerializable
{
    public byte SelectionNumber;
    public HomeEventType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SelectionNumber);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class HomeEventMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<HomeEventData>();

        var controller = HomeEventHandler.GameController;

        if (!controller)
        {
            return;
        }

        GameControl_OutroPatches.IgnorePatches = true;
        TaxiControllerPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case HomeEventType.UNKNOWN:
                    break;
                case HomeEventType.WARMUP_JIMMY_ARM:
                    controller.WarmUpJimmyArm();
                    break;
                case HomeEventType.REACH_WINDMILL:
                    controller.ReachWindmill();
                    break;
                case HomeEventType.REACHED_TAXI:
                    controller.ReachedTaxi();
                    break;
                case HomeEventType.ARM_HIDE:
                    controller.ArmHide();

                    HomeEventHandler.TeleportToJimmyFinger();
                    break;
                case HomeEventType.DRIVING_END:
                    controller.DrivingEnd();
                    break;
                case HomeEventType.COMPLETE_GAME:
                    controller.CompleteGame();
                    break;
                case HomeEventType.SPLINE_LOOP_COUNTER:
                    HomeEventHandler.TaxiController.SplineLoopCounter();
                    break;
            }
        }
        catch (Exception e) 
        {
            FusionLogger.LogException("handling HomeMessage", e);
        }

        GameControl_OutroPatches.IgnorePatches = false;
        TaxiControllerPatches.IgnorePatches = false;
    }
}
