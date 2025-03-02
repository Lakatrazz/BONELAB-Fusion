using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum KartRaceEventType
{
    UNKNOWN = 0,
    START_RACE = 1,
    NEW_LAP = 2,
    RESET_RACE = 3,
    END_RACE = 4,
}

public class KartRaceEventData : INetSerializable
{
    public byte smallId;
    public KartRaceEventType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static KartRaceEventData Create(byte smallId, KartRaceEventType type)
    {
        return new KartRaceEventData()
        {
            smallId = smallId,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class KartRaceEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.KartRaceEvent;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<KartRaceEventData>();

        KartRacePatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case KartRaceEventType.UNKNOWN:
                break;
            case KartRaceEventType.START_RACE:
                KartRaceData.GameController.STARTRACE();
                break;
            case KartRaceEventType.NEW_LAP:
                KartRaceData.GameController.NEWLAP();
                break;
            case KartRaceEventType.RESET_RACE:
                KartRaceData.GameController.RESETRACE();
                break;
            case KartRaceEventType.END_RACE:
                KartRaceData.GameController.ENDRACE();
                break;
        }

        KartRacePatches.IgnorePatches = false;
    }
}
