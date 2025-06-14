using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum BaseGameControllerType
{
    UNKNOWN = 0,
    BeginSession = 1,
    EndSession = 2,
}

public class BaseGameControllerData : INetSerializable
{
    public BaseGameControllerType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class BaseGameControllerMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BaseGameControllerData>();

        if (!GameControllerEventHandler.HasGameController)
        {
            return;
        }

        BaseGameControllerPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case BaseGameControllerType.UNKNOWN:
                    break;
                case BaseGameControllerType.BeginSession:
                    GameControllerEventHandler.GameController.BeginSession();
                    break;
                case BaseGameControllerType.EndSession:
                    GameControllerEventHandler.GameController.EndSession();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling BaseGameControllerMessage", e);
        }

        BaseGameControllerPatches.IgnorePatches = false;
    }
}
