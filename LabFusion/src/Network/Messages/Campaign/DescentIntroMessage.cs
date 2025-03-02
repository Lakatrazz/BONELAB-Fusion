using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum DescentIntroType
{
    UNKNOWN = 0,
    SEQUENCE = 1,
    BUTTON_CONFIRM = 2,
    CONFIRM_FORCE_GRAB = 3,
}

public class DescentIntroData : INetSerializable
{
    public byte smallId;
    public byte selectionNumber;
    public DescentIntroType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref selectionNumber);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static DescentIntroData Create(byte smallId, byte selectionNumber, DescentIntroType type)
    {
        return new DescentIntroData()
        {
            smallId = smallId,
            selectionNumber = selectionNumber,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class DescentIntroMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DescentIntro;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DescentIntroData>();

        GameControl_DescentPatches.IgnorePatches = true;
        Control_UI_BodyMeasurementsPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case DescentIntroType.UNKNOWN:
                break;
            case DescentIntroType.SEQUENCE:
                DescentData.GameController.SEQUENCE(data.selectionNumber);
                break;
            case DescentIntroType.BUTTON_CONFIRM:
                DescentData.BodyMeasurementsUI.BUTTON_CONFIRM();
                break;
            case DescentIntroType.CONFIRM_FORCE_GRAB:
                DescentData.GameController.CONFIRMFORCEGRAB();
                break;
        }

        GameControl_DescentPatches.IgnorePatches = false;
        Control_UI_BodyMeasurementsPatches.IgnorePatches = false;
    }
}
