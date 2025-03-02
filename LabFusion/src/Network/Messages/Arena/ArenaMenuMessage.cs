using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum ArenaMenuType
{
    UNKNOWN = 0,
    CHALLENGE_SELECT = 1,
    TRIAL_SELECT = 2,
    SURVIVAL_SELECT = 3,
    TOGGLE_DIFFICULTY = 4,
    TOGGLE_ENEMY_PROFILE = 5,
    CREATE_CUSTOM_GAME_AND_START = 6,
    RESUME_SURVIVAL_FROM_ROUND = 7,
}

public class ArenaMenuData : INetSerializable
{
    public byte selectionNumber;
    public ArenaMenuType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref selectionNumber);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static ArenaMenuData Create(byte selectionNumber, ArenaMenuType type)
    {
        return new ArenaMenuData()
        {
            selectionNumber = selectionNumber,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ArenaMenuMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ArenaMenu;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ArenaMenuData>();
        var menu = ArenaData.MenuController;

        if (!menu)
        {
            return;
        }

        ArenaMenuPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case ArenaMenuType.UNKNOWN:
                break;
            case ArenaMenuType.CHALLENGE_SELECT:
                menu.ChallengeSelect(data.selectionNumber);
                break;
            case ArenaMenuType.TRIAL_SELECT:
                menu.TrialSelect(data.selectionNumber);
                break;
            case ArenaMenuType.SURVIVAL_SELECT:
                menu.SurvivalSelect();
                break;
            case ArenaMenuType.TOGGLE_DIFFICULTY:
                menu.ToggleDifficulty(data.selectionNumber);
                break;
            case ArenaMenuType.TOGGLE_ENEMY_PROFILE:
                menu.ToggleEnemyProfile(data.selectionNumber);
                break;
            case ArenaMenuType.CREATE_CUSTOM_GAME_AND_START:
                menu.CreateCustomGameAndStart();
                break;
            case ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND:
                menu.ResumeSurvivalFromRound();
                break;
        }

        ArenaMenuPatches.IgnorePatches = false;
    }
}
