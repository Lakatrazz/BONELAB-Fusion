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
        return SendArenaTransition(ArenaTransitionType.ARENA_PlayerEnter);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.InitObjectiveContainer))]
    public static bool InitObjectiveContainer()
    {
        return SendArenaTransition(ArenaTransitionType.InitObjectiveContainer);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_StartMatch))]
    public static bool ARENA_StartMatch()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_StartMatch);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.StartNextWave))]
    public static bool StartNextWave()
    {
        return SendArenaTransition(ArenaTransitionType.StartNextWave);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_QuitChallenge))]
    public static bool ARENA_QuitChallenge()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_QuitChallenge);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_CancelMatch))]
    public static bool ARENA_CancelMatch()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_CancelMatch);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_ResetTheBell))]
    public static bool ARENA_ResetTheBell()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_ResetTheBell);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.ARENA_RingTheBell))]
    public static bool ARENA_RingTheBell()
    {
        return SendArenaTransition(ArenaTransitionType.ARENA_RingTheBell);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.FailObjectiveMode))]
    public static bool FailObjectiveMode()
    {
        return SendArenaTransition(ArenaTransitionType.FailObjectiveMode);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.FailEscapeMode))]
    public static bool FailEscapeMode()
    {
        return SendArenaTransition(ArenaTransitionType.FailEscapeMode);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.SpawnLoot))]
    public static bool SpawnLoot()
    {
        return SendArenaTransition(ArenaTransitionType.SpawnLoot);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Arena_GameController.StartSpawning))]
    public static bool StartSpawning()
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // If not the level host, cancel StartSpawning and consequently the SpawnEnemyLoop
        // This can lead to freezes from the game controller state sometimes not matching the host's state (which SpawnEnemyLoop logs constantly for some reason)
        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        return true;
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

        MessageRelay.RelayModule<ArenaTransitionMessage, ArenaTransitionData>(new ArenaTransitionData() { Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
