using HarmonyLib;

using Il2CppSLZ.Bonelab;

using MarrowFusion.Bonelab.Extenders;
using LabFusion.Network;
using MarrowFusion.Bonelab.Messages;

using Random = UnityEngine.Random;

namespace MarrowFusion.Bonelab.Patching;

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

        // If we're the owner of this RandomObject, manually determine an object from the list and sync it
        if (entity.IsOwner)
        {
            ushort objectIndex = (ushort)Random.Range(0, __instance.Objects.Count);

            // Send the message to sync it
            var data = RandomObjectData.Create(entity.ID, extender.GetIndex(__instance).Value, objectIndex);

            MessageRelay.RelayModule<RandomObjectMessage, RandomObjectData>(data, CommonMessageRoutes.ReliableToClients);
        }

        // On any synced RandomObjects, always return false. It's manually applied by the message.
        return false;
    }
}