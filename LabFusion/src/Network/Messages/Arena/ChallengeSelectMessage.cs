using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum ChallengeSelectType
{
    UNKNOWN = 0,
    SELECT_CHALLENGE = 1,
    ON_CHALLENGE_SELECT = 2,
}

public class ChallengeSelectData : INetSerializable
{
    public byte menuIndex;
    public byte challengeNumber;
    public ChallengeSelectType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref menuIndex);
        serializer.SerializeValue(ref challengeNumber);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static ChallengeSelectData Create(byte menuIndex, byte challengeNumber, ChallengeSelectType type)
    {
        return new ChallengeSelectData()
        {
            menuIndex = menuIndex,
            challengeNumber = challengeNumber,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ChallengeSelectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ChallengeSelect;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ChallengeSelectData>();
        var menu = ArenaData.GetMenu(data.menuIndex);

        if (!menu)
        {
            return;
        }

        ChallengePatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case ChallengeSelectType.UNKNOWN:
                break;
            case ChallengeSelectType.SELECT_CHALLENGE:
                menu.SelectChallenge(data.challengeNumber);
                break;
            case ChallengeSelectType.ON_CHALLENGE_SELECT:
                menu.OnChallengeSelect();
                break;
        }

        ChallengePatches.IgnorePatches = false;
    }
}
