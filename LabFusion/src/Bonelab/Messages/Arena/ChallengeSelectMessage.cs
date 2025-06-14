using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Network;

namespace LabFusion.Bonelab.Messages;

public enum ChallengeSelectType
{
    UNKNOWN = 0,
    SELECT_CHALLENGE = 1,
    ON_CHALLENGE_SELECT = 2,
}

public class ChallengeSelectData : INetSerializable
{
    public byte MenuIndex;
    public byte ChallengeNumber;
    public ChallengeSelectType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref MenuIndex);
        serializer.SerializeValue(ref ChallengeNumber);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }

    public static ChallengeSelectData Create(byte menuIndex, byte challengeNumber, ChallengeSelectType type)
    {
        return new ChallengeSelectData()
        {
            MenuIndex = menuIndex,
            ChallengeNumber = challengeNumber,
            Type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ChallengeSelectMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ChallengeSelectData>();
        var menu = ArenaEventHandler.GetMenu(data.MenuIndex);

        if (!menu)
        {
            return;
        }

        ChallengeSelectMenuPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case ChallengeSelectType.UNKNOWN:
                    break;
                case ChallengeSelectType.SELECT_CHALLENGE:
                    menu.SelectChallenge(data.ChallengeNumber);
                    break;
                case ChallengeSelectType.ON_CHALLENGE_SELECT:
                    menu.OnChallengeSelect();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling ChallengeSelectMessage", e);
        }

        ChallengeSelectMenuPatches.IgnorePatches = false;
    }
}
