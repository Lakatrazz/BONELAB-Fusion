using HarmonyLib;

using Il2CppSLZ.Marrow.Circuits;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Actuator))]
public static class ActuatorPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Actuator.Awake))]
    public static void Awake(Actuator __instance)
    {
        var eventActuator = __instance.TryCast<EventActuator>();

        if (eventActuator != null)
        {
            EventActuatorPatches.Awake(eventActuator);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Actuator.OnDestroy))]
    public static void OnDestroy(Actuator __instance)
    {
        var eventActuator = __instance.TryCast<EventActuator>();

        if (eventActuator != null)
        {
            EventActuatorPatches.OnDestroy(eventActuator);
        }
    }
}
