using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Messages;
using LabFusion.Network;
using LabFusion.Scene;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(ArenaMenuController))]
public static class ArenaMenuControllerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.ChallengeSelect))]
    public static bool ChallengeSelect(ArenaMenuController __instance, int sel)
    {
        return SendArenaMenu((byte)sel, ArenaMenuType.CHALLENGE_SELECT);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.TrialSelect))]
    public static bool TrialSelect(ArenaMenuController __instance, int sel)
    {
        return SendArenaMenu((byte)sel, ArenaMenuType.TRIAL_SELECT);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.SurvivalSelect))]
    public static bool SurvivalSelect(ArenaMenuController __instance)
    {
        return SendArenaMenu(0, ArenaMenuType.SURVIVAL_SELECT);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.ToggleDifficulty))]
    public static bool ToggleDifficulty(ArenaMenuController __instance, int diff)
    {
        return SendArenaMenu((byte)diff, ArenaMenuType.TOGGLE_DIFFICULTY);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.ToggleEnemyProfile))]
    public static bool ToggleEnemyProfile(ArenaMenuController __instance, int profileIndex)
    {
        return SendArenaMenu((byte)profileIndex, ArenaMenuType.TOGGLE_ENEMY_PROFILE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.CreateCustomGameAndStart))]
    public static bool CreateCustomGameAndStart(ArenaMenuController __instance)
    {
        return SendArenaMenu(0, ArenaMenuType.CREATE_CUSTOM_GAME_AND_START);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArenaMenuController.ResumeSurvivalFromRound))]
    public static bool ResumeSurvivalFromRound(ArenaMenuController __instance)
    {
        return SendArenaMenu(0, ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND);
    }

    private static bool SendArenaMenu(byte selectionNumber, ArenaMenuType type)
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

        MessageRelay.RelayModule<ArenaMenuMessage, ArenaMenuData>(new ArenaMenuData() { SelectionNumber = selectionNumber, Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}