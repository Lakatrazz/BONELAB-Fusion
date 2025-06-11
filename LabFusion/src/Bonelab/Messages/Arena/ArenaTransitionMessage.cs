using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum ArenaTransitionType
{
    UNKNOWN,
    ARENA_PlayerEnter,
    InitObjectiveContainer,
    ARENA_StartMatch,
    StartNextWave,
    ARENA_QuitChallenge,
    ARENA_CancelMatch,
    ARENA_ResetTheBell,
    ARENA_RingTheBell,
    FailObjectiveMode,
    FailEscapeMode,
    SpawnLoot,
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
                case ArenaTransitionType.ARENA_PlayerEnter:
                    ArenaEventHandler.GameController.ARENA_PlayerEnter();
                    break;
                case ArenaTransitionType.InitObjectiveContainer:
                    ArenaEventHandler.GameController.InitObjectiveContainer();

                    if (ArenaEventHandler.GameControlDisplay)
                    {
                        ArenaEventHandler.GameControlDisplay.gameObject.SetActive(true);
                    }
                    break;
                case ArenaTransitionType.ARENA_StartMatch:
                    ArenaEventHandler.GameController.ARENA_StartMatch();
                    break;
                case ArenaTransitionType.StartNextWave:
                    ArenaEventHandler.GameController.StartNextWave();
                    break;
                case ArenaTransitionType.ARENA_QuitChallenge:
                    ArenaEventHandler.GameController.ARENA_QuitChallenge();
                    break;
                case ArenaTransitionType.ARENA_CancelMatch:
                    ArenaEventHandler.GameController.ARENA_CancelMatch();
                    break;
                case ArenaTransitionType.ARENA_ResetTheBell:
                    ArenaEventHandler.GameController.ARENA_ResetTheBell();
                    break;
                case ArenaTransitionType.ARENA_RingTheBell:
                    ArenaEventHandler.GameController.ARENA_RingTheBell();
                    break;
                case ArenaTransitionType.FailObjectiveMode:
                    ArenaEventHandler.GameController.FailObjectiveMode();
                    break;
                case ArenaTransitionType.FailEscapeMode:
                    ArenaEventHandler.GameController.FailEscapeMode();
                    break;
                case ArenaTransitionType.SpawnLoot:
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