using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Extenders;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;

using Random = UnityEngine.Random;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(RandomObject))]
public static class RandomObjectPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RandomObject.Randomizer))]
    public static bool RandomizerPrefix(RandomObject __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var entity = RandomObjectExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return true;
        }

        var extender = entity.GetExtender<RandomObjectExtender>();

        // If we're the server, manually determine an object from the list and sync it
        if (NetworkInfo.IsHost)
        {
            ushort objectIndex = (ushort)Random.Range(0, __instance.Objects.Count);

            // Send the message to sync it
            var data = RandomObjectData.Create(entity.ID, extender.GetIndex(__instance).Value, objectIndex);

            MessageRelay.RelayModule<RandomObjectMessage, RandomObjectData>(data, CommonMessageRoutes.ReliableToOtherClients);
        }

        // On any synced RandomObjects, always return false. It's manually applied by the message.
        return false;
    }
}