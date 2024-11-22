using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Entities;
using LabFusion.Network;

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
        
        // Send message to move entity's active culler
        using var writer = FusionWriter.Create(EntityZoneRegisterData.Size);
        var data = EntityZoneRegisterData.Create(networkEntity.OwnerId, networkEntity.Id);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.EntityZoneRegister, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}