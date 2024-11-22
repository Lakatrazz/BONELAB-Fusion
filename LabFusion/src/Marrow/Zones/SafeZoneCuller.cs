using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Marrow.Zones;

public static class SafeZoneCuller
{
    public static void Cull(MarrowEntity entity, bool isInactive = true)
    {
        ZoneManagerPlugin.InactiveManager.Cull(entity._inactiveStatus, isInactive);
    }
}