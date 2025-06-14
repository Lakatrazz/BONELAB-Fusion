using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.Bonelab.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum DescentIntroType
{
    UNKNOWN = 0,
    SEQUENCE = 1,
    BUTTON_CONFIRM = 2,
    CONFIRM_FORCE_GRAB = 3,
}

public class DescentIntroData : INetSerializable
{
    public byte SelectionNumber;
    public DescentIntroType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SelectionNumber);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class DescentIntroMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DescentIntroData>();

        GameControl_DescentPatches.IgnorePatches = true;
        Control_UI_BodyMeasurementsPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case DescentIntroType.UNKNOWN:
                    break;
                case DescentIntroType.SEQUENCE:
                    DescentEventHandler.GameController.SEQUENCE(data.SelectionNumber);
                    break;
                case DescentIntroType.BUTTON_CONFIRM:
                    DescentEventHandler.BodyMeasurementsUI.BUTTON_CONFIRM();
                    break;
                case DescentIntroType.CONFIRM_FORCE_GRAB:
                    DescentEventHandler.GameController.CONFIRMFORCEGRAB();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling DescentIntroMessage", e);
        }

        GameControl_DescentPatches.IgnorePatches = false;
        Control_UI_BodyMeasurementsPatches.IgnorePatches = false;
    }
}
