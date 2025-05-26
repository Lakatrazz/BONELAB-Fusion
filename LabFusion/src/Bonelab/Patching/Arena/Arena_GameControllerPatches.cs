using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(Arena_GameController))]
public static class Arena_GameControllerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_PlayerEnter))]
    public static bool ARENA_PlayerEnter()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_PLAYER_ENTER);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.InitObjectiveContainer))]
    public static bool InitObjectiveContainer()
    {
        return SendArenaTransition(ArenaTransitionType.INIT_OBJECTIVE_CONTAINER);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_StartMatch))]
    public static bool ARENA_StartMatch()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_START_MATCH);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.StartNextWave))]
    public static bool StartNextWave()
    {
        return SendArenaTransition(ArenaTransitionType.START_NEXT_WAVE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_QuitChallenge))]
    public static bool ARENA_QuitChallenge()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_QUIT_CHALLENGE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_CancelMatch))]
    public static bool ARENA_CancelMatch()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_CANCEL_MATCH);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_ResetTheBell))]
    public static bool ARENA_ResetTheBell()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_RESET_THE_BELL);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_RingTheBell))]
    public static bool ARENA_RingTheBell()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_RING_THE_BELL);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.FailObjectiveMode))]
    public static bool FailObjectiveMode()
    {
        return SendArenaTransition(ArenaTransitionType.FAIL_OBJECTIVE_MODE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.FailEscapeMode))]
    public static bool FailEscapeMode()
    {
        return SendArenaTransition(ArenaTransitionType.FAIL_ESCAPE_MODE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.SpawnLoot))]
    public static bool SpawnLoot()
    {
        return SendArenaTransition(ArenaTransitionType.SPAWN_LOOT);
    }

    private static bool SendArenaTransition(ArenaTransitionType type)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        MessageRelay.RelayModule<ArenaTransitionMessage, ArenaTransitionData>(new ArenaTransitionData() { Type = type }, NetworkChannel.Reliable, RelayType.ToOtherClients);
        return true;
    }
}
