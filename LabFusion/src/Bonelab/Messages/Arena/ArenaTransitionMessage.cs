using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum ArenaTransitionType
{
    UNKNOWN = 0,
    ARENA_PLAYER_ENTER = 1,
    INIT_OBJECTIVE_CONTAINER = 2,
    ARENA_START_MATCH = 3,
    START_NEXT_WAVE = 4,
    ARENA_QUIT_CHALLENGE = 5,
    ARENA_CANCEL_MATCH = 6,
    ARENA_RESET_THE_BELL = 7,
    ARENA_RING_THE_BELL = 8,
    FAIL_OBJECTIVE_MODE = 9,
    FAIL_ESCAPE_MODE = 10,
    SPAWN_LOOT = 11,
}

public class ArenaTransitionData : INetSerializable
{
    public ArenaTransitionType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class ArenaTransitionMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ArenaTransitionData>();

        if (!ArenaEventHandler.IsInArena)
        {
            return;
        }

        Arena_GameControllerPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case ArenaTransitionType.UNKNOWN:
                    break;
                case ArenaTransitionType.ARENA_PLAYER_ENTER:
                    ArenaEventHandler.GameController.ARENA_PlayerEnter();
                    break;
                case ArenaTransitionType.INIT_OBJECTIVE_CONTAINER:
                    ArenaEventHandler.GameController.InitObjectiveContainer();

                    if (ArenaEventHandler.GameControlDisplay)
                    {
                        ArenaEventHandler.GameControlDisplay.gameObject.SetActive(true);
                    }
                    break;
                case ArenaTransitionType.ARENA_START_MATCH:
                    ArenaEventHandler.GameController.ARENA_StartMatch();
                    break;
                case ArenaTransitionType.START_NEXT_WAVE:
                    ArenaEventHandler.GameController.StartNextWave();
                    break;
                case ArenaTransitionType.ARENA_QUIT_CHALLENGE:
                    ArenaEventHandler.GameController.ARENA_QuitChallenge();
                    break;
                case ArenaTransitionType.ARENA_CANCEL_MATCH:
                    ArenaEventHandler.GameController.ARENA_CancelMatch();
                    break;
                case ArenaTransitionType.ARENA_RESET_THE_BELL:
                    ArenaEventHandler.GameController.ARENA_ResetTheBell();
                    break;
                case ArenaTransitionType.ARENA_RING_THE_BELL:
                    ArenaEventHandler.GameController.ARENA_RingTheBell();
                    break;
                case ArenaTransitionType.FAIL_OBJECTIVE_MODE:
                    ArenaEventHandler.GameController.FailObjectiveMode();
                    break;
                case ArenaTransitionType.FAIL_ESCAPE_MODE:
                    ArenaEventHandler.GameController.FailEscapeMode();
                    break;
                case ArenaTransitionType.SPAWN_LOOT:
                    ArenaEventHandler.GameController.SpawnLoot();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling ArenaTransitionMessage", e);
        }

        Arena_GameControllerPatches.IgnorePatches = false;
    }
}