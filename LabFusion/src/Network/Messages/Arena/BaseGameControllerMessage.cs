using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum BaseGameControllerType
{
    UNKNOWN = 0,
    BeginSession = 1,
    EndSession = 2,
}

public class BaseGameControllerData : IFusionSerializable
{
    public BaseGameControllerType type;

    public void Serialize(FusionWriter writer)
    {
        writer.Write((byte)type);
    }

    public void Deserialize(FusionReader reader)
    {
        type = (BaseGameControllerType)reader.ReadByte();
    }

    public static BaseGameControllerData Create(BaseGameControllerType type)
    {
        return new BaseGameControllerData()
        {
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class BaseGameControllerMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.BaseGameController;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BaseGameControllerData>();

        if (!GameControllerData.HasGameController)
        {
            return;
        }

        BaseGameControllerPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case BaseGameControllerType.UNKNOWN:
                break;
            case BaseGameControllerType.BeginSession:
                GameControllerData.GameController.BeginSession();
                break;
            case BaseGameControllerType.EndSession:
                GameControllerData.GameController.EndSession();
                break;
        }

        BaseGameControllerPatches.IgnorePatches = false;
    }
}
