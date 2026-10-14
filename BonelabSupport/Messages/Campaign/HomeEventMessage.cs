using MarrowFusion.Bonelab.Scene;
using MarrowFusion.Bonelab.Patching;

using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace MarrowFusion.Bonelab.Messages;

public enum HomeEventType
{
    None = 0,
    WarmupJimmyArm = 1,
    ReachWindmill = 2,
    ReachedTaxi = 3,
    ArmHide = 4,
    DrivingEnd = 5,
    CompleteGame = 6,
    SplineLoopCounter = 7,
}

public class HomeEventData : INetSerializable
{
    public int? GetSize() => sizeof(byte);

    public HomeEventType Type;

    public void Serialize(INetSerializer serializer)
    {
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
                case HomeEventType.None:
                    break;
                case HomeEventType.WarmupJimmyArm:
                    controller.WarmUpJimmyArm();
                    break;
                case HomeEventType.ReachWindmill:
                    controller.ReachWindmill();
                    break;
                case HomeEventType.ReachedTaxi:
                    controller.ReachedTaxi();
                    break;
                case HomeEventType.ArmHide:
                    controller.ArmHide();

                    HomeEventHandler.TeleportToJimmyFinger();
                    break;
                case HomeEventType.DrivingEnd:
                    controller.DrivingEnd();
                    break;
                case HomeEventType.CompleteGame:
                    controller.CompleteGame();
                    break;
                case HomeEventType.SplineLoopCounter:
                    HomeEventHandler.TaxiController.SplineLoopCounter();
                    break;
            }
        }
        catch (Exception e) 
        {
            BonelabModule.Logger.LogException("handling HomeMessage", e);
        }

        GameControl_OutroPatches.IgnorePatches = false;
        TaxiControllerPatches.IgnorePatches = false;
    }
}
