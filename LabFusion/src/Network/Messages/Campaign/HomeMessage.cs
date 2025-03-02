using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

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
    public byte selectionNumber;
    public HomeEventType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref selectionNumber);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static HomeEventData Create(byte selectionNumber, HomeEventType type)
    {
        return new HomeEventData()
        {
            selectionNumber = selectionNumber,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class HomeEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.HomeEvent;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<HomeEventData>();

        var controller = HomeData.GameController;

        HomePatches.IgnorePatches = true;
        TaxiControllerPatches.IgnorePatches = true;

        if (controller)
        {
            switch (data.type)
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

                    HomeData.TeleportToJimmyFinger();
                    break;
                case HomeEventType.DRIVING_END:
                    controller.DrivingEnd();
                    break;
                case HomeEventType.COMPLETE_GAME:
                    controller.CompleteGame();
                    break;
                case HomeEventType.SPLINE_LOOP_COUNTER:
                    HomeData.TaxiController.SplineLoopCounter();
                    break;
            }
        }

        HomePatches.IgnorePatches = false;
        TaxiControllerPatches.IgnorePatches = false;
    }
}
