using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(ZoneCullManager))]
public static class ZoneCullManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZoneCullManager.Register))]
    [HarmonyPatch(new Type[]
    {
        typeof(int),
        typeof(MarrowEntity)
    })]
    public static void Register(int cullerId, MarrowEntity entity)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Check if this entity is networked and we have ownership
        var networkEntity = IMarrowEntityExtender.Cache.Get(entity);

        if (networkEntity == null || !networkEntity.IsOwner)
        {
            return;
        }
        
        // Try and find the zone hash from its culler id
        if (!ZoneCullerPatches.CullerIdToZone.TryGetValue(cullerId, out var zoneCuller))
        {
            FusionLogger.Warn($"cullerId {cullerId} was not in the dictionary?");
            return;
        }

        if (!ZoneCullerPatches.ZoneToHash.TryGetValue(zoneCuller, out var hash))
        {
            FusionLogger.Warn($"Zone {zoneCuller.name} was not in the dictionary?");
            return;
        }

        // Send message to move entity's active culler
        using var writer = FusionWriter.Create(EntityZoneRegisterData.Size);
        var data = EntityZoneRegisterData.Create(networkEntity.OwnerId, networkEntity.Id, hash);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.EntityZoneRegister, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}