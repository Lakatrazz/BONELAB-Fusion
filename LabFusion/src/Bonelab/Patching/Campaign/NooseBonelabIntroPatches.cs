using HarmonyLib;

using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Player;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(NooseBonelabIntro))]
public static class NooseBonelabIntroPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NooseBonelabIntro.AttachNeck))]
    public static void AttachNeck()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var nooseEvent = DescentEventHandler.CreateNooseEvent(PlayerIDManager.LocalSmallID, DescentNooseType.ATTACH_NOOSE);

        MessageRelay.RelayModule<DescentNooseMessage, DescentNooseData>(new DescentNooseData() { PlayerId = nooseEvent.PlayerId, Type = nooseEvent.Type }, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NooseBonelabIntro.NooseCut))]
    public static void NooseCut()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var nooseEvent = DescentEventHandler.CreateNooseEvent(PlayerIDManager.LocalSmallID, DescentNooseType.CUT_NOOSE);

        MessageRelay.RelayModule<DescentNooseMessage, DescentNooseData>(new DescentNooseData() { PlayerId = nooseEvent.PlayerId, Type = nooseEvent.Type }, CommonMessageRoutes.ReliableToOtherClients);
    }
}
