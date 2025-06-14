using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum HolodeckEventType
{
    UNKNOWN = 0,
    TOGGLE_DOOR = 1,
    SELECT_MATERIAL = 2,
}

public class HolodeckEventData : INetSerializable
{
    public HolodeckEventType Type;
    public int SelectionIndex;
    public bool ToggleValue;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref SelectionIndex);
        serializer.SerializeValue(ref ToggleValue);
    }
}

[Net.DelayWhileTargetLoading]
public class HolodeckEventMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<HolodeckEventData>();

        var deck = HoloChamberEventHandler.GameController;

        if (deck == null)
        {
            return;
        }

        GameControl_HolodeckPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case HolodeckEventType.UNKNOWN:
                    break;
                case HolodeckEventType.TOGGLE_DOOR:
                    deck.doorHide.SetActive(!data.ToggleValue);
                    deck.TOGGLEDOOR();
                    break;
                case HolodeckEventType.SELECT_MATERIAL:
                    deck.SELECTMATERIAL(data.SelectionIndex);
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling HolodeckEventMessage", e);
        }

        GameControl_HolodeckPatches.IgnorePatches = false;
    }
}
