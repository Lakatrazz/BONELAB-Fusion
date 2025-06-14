using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Scene;
using LabFusion.Bonelab.Messages;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(ChallengeSelectMenu))]
public static class ChallengeSelectMenuPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChallengeSelectMenu.OnChallengeSelect))]
    public static bool OnChallengeSelect(ChallengeSelectMenu __instance)
    {
        return SendChallengeSelect(ArenaEventHandler.GetIndex(__instance).Value, 0, ChallengeSelectType.ON_CHALLENGE_SELECT);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChallengeSelectMenu.SelectChallenge))]
    public static bool SelectChallenge(ChallengeSelectMenu __instance, int idx)
    {
        return SendChallengeSelect(ArenaEventHandler.GetIndex(__instance).Value, idx, ChallengeSelectType.SELECT_CHALLENGE);
    }

    private static bool SendChallengeSelect(int menuIndex, int challengeNumber, ChallengeSelectType type)
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

        MessageRelay.RelayModule<ChallengeSelectMessage, ChallengeSelectData>(new ChallengeSelectData() { MenuIndex = (byte)menuIndex, ChallengeNumber = (byte)challengeNumber, Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}