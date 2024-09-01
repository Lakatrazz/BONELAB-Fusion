using System.Reflection;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;

using HarmonyLib;

namespace LabFusion.Patching;

public static class VirtualControllerPatches
{
    internal static void Patch(HarmonyLib.Harmony harmonyInstace)
    {
        if (PlatformHelper.IsAndroid)
            return;

        MethodBase methodBase = AccessTools.Method(typeof(VirtualController), nameof(VirtualController.CheckHandDesync));

        Type[] types = new Type[] { typeof(HandGripPair), typeof(SimpleTransform), typeof(SimpleTransform) };

        HarmonyMethod harmonyPrefix = new HarmonyMethod(typeof(VirtualControllerPatches), nameof(CheckHandDesync), types);

        harmonyInstace.Patch(methodBase, harmonyPrefix);
    }

    public static bool CheckHandDesync(HandGripPair pair, SimpleTransform contHandle, SimpleTransform rigHandle)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // The hand and its rigManager will never be null, so we don't need to check for it
        // If they are null, then something has seriously gone wrong, so an error *should* be thrown and not hidden
        var hand = pair.hand;

        if (NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        return true;
    }
}