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
        var data = EntityZoneRegisterData.Create(networkEntity.OwnerId, networkEntity.Id);

        MessageRelay.RelayNative(data, NativeMessageTag.EntityZoneRegister, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}