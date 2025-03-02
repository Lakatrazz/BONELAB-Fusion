using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum HolodeckEventType
{
    UNKNOWN = 0,
    TOGGLE_DOOR = 1,
    SELECT_MATERIAL = 2,
}

public class HolodeckEventData : INetSerializable
{
    public byte smallId;
    public HolodeckEventType type;
    public int selectionIndex;
    public bool toggleValue;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref selectionIndex);
        serializer.SerializeValue(ref toggleValue);
    }

    public static HolodeckEventData Create(byte smallId, HolodeckEventType type, int selectionIndex, bool toggleValue)
    {
        return new HolodeckEventData()
        {
            smallId = smallId,
            type = type,
            selectionIndex = selectionIndex,
            toggleValue = toggleValue,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class HolodeckEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.HolodeckEvent;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<HolodeckEventData>();

        var deck = HolodeckData.GameController;

        GameControl_HolodeckPatches.IgnorePatches = true;

        if (deck != null)
        {
            switch (data.type)
            {
                default:
                case HolodeckEventType.UNKNOWN:
                    break;
                case HolodeckEventType.TOGGLE_DOOR:
                    deck.doorHide.SetActive(!data.toggleValue);
                    deck.TOGGLEDOOR();
                    break;
                case HolodeckEventType.SELECT_MATERIAL:
                    deck.SELECTMATERIAL(data.selectionIndex);
                    break;
            }
        }

        GameControl_HolodeckPatches.IgnorePatches = false;
    }
}
