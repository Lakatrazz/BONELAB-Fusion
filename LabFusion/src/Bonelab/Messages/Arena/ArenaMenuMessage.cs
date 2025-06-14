using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

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
    public byte SelectionNumber;
    public ArenaMenuType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SelectionNumber);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class ArenaMenuMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ArenaMenuData>();
        var menu = ArenaEventHandler.MenuController;

        if (!menu)
        {
            return;
        }

        ArenaMenuControllerPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case ArenaMenuType.UNKNOWN:
                    break;
                case ArenaMenuType.CHALLENGE_SELECT:
                    menu.ChallengeSelect(data.SelectionNumber);
                    break;
                case ArenaMenuType.TRIAL_SELECT:
                    menu.TrialSelect(data.SelectionNumber);
                    break;
                case ArenaMenuType.SURVIVAL_SELECT:
                    menu.SurvivalSelect();
                    break;
                case ArenaMenuType.TOGGLE_DIFFICULTY:
                    menu.ToggleDifficulty(data.SelectionNumber);
                    break;
                case ArenaMenuType.TOGGLE_ENEMY_PROFILE:
                    menu.ToggleEnemyProfile(data.SelectionNumber);
                    break;
                case ArenaMenuType.CREATE_CUSTOM_GAME_AND_START:
                    menu.CreateCustomGameAndStart();
                    break;
                case ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND:
                    menu.ResumeSurvivalFromRound();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling ArenaMenuMessage", e);
        }

        ArenaMenuControllerPatches.IgnorePatches = false;
    }
}
