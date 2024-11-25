using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Extenders;
using LabFusion.Network;
using LabFusion.Player;

using Random = UnityEngine.Random;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(RandomObject))]
public static class RandomObjectPatches
{
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
        if (NetworkInfo.IsServer)
        {
            ushort objectIndex = (ushort)Random.Range(0, __instance.Objects.Count);

            // Send the message to sync it
            using var writer = FusionWriter.Create(RandomObjectData.Size);
            var data = RandomObjectData.Create(PlayerIdManager.LocalSmallId, entity.Id, extender.GetIndex(__instance).Value, objectIndex);
            writer.Write(data);

            using var message = FusionMessage.ModuleCreate<RandomObjectMessage>(writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        // On any synced RandomObjects, always return false. It's manually applied by the message.
        return false;
    }
}