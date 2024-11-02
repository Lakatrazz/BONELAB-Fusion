using HarmonyLib;

using Il2CppSLZ.Marrow.Circuits;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

using MelonLoader.NativeUtils;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(EventActuator))]
public static class EventActuatorPatches
{
    public static readonly ComponentHashTable<EventActuator> HashTable = new();

    public static void Awake(EventActuator actuator)
    {
        var hash = GameObjectHasher.GetHierarchyHash(actuator.gameObject);

        var index = HashTable.AddComponent(hash, actuator);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"EventActuator {actuator.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif

        // Hook into UltEvents
        // We don't need to do this if the UltEvent doesn't have calls
        if (actuator.InputUpdated.HasCalls)
        {
            actuator.InputUpdated.add_DynamicCalls((Il2CppSystem.Action<float>)((float f) => {
                OnInputUpdated(actuator, f);
            }));
        }

        if (actuator.InputRose.HasCalls)
        {
            actuator.InputRose.add_DynamicCalls((Il2CppSystem.Action<float>)((float f) => {
                OnInputRose(actuator, f);
            }));
        }

        if (actuator.InputHeld.HasCalls)
        {
            actuator.InputHeld.add_DynamicCalls((Il2CppSystem.Action<float>)((float f) => {
                OnInputHeld(actuator, f);
            }));
        }

        if (actuator.InputFell.HasCalls)
        {
            actuator.InputFell.add_DynamicCalls((Il2CppSystem.Action<float>)((float f) => {
                OnInputFell(actuator, f);
            }));
        }

        if (actuator.InputRoseOneShot.HasCalls)
        {
            actuator.InputRoseOneShot.add_DynamicCalls((Il2CppSystem.Action<float>)((float f) => {
                OnInputRoseOneShot(actuator, f);
            }));
        }
    }

    public static void OnDestroy(EventActuator actuator)
    {
        HashTable.RemoveComponent(actuator);
    }

    private static void OnInputUpdated(EventActuator actuator, float f)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        OnSendEvent(actuator, EventActuatorType.UPDATED, f, NetworkChannel.Unreliable);
    }

    private static void OnInputRose(EventActuator actuator, float f)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        OnSendEvent(actuator, EventActuatorType.ROSE, f, NetworkChannel.Reliable);
    }

    private static void OnInputHeld(EventActuator actuator, float f)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        OnSendEvent(actuator, EventActuatorType.HELD, f, NetworkChannel.Unreliable);
    }

    private static void OnInputFell(EventActuator actuator, float f)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        OnSendEvent(actuator, EventActuatorType.FELL, f, NetworkChannel.Reliable);
    }

    private static void OnInputRoseOneShot(EventActuator actuator, float f)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        OnSendEvent(actuator, EventActuatorType.ROSEONESHOT, f, NetworkChannel.Reliable);
    }

    private static void OnSendEvent(EventActuator actuator, EventActuatorType type, float value, NetworkChannel channel)
    {
        var hashData = HashTable.GetDataFromComponent(actuator);

        if (hashData == null)
        {
            return;
        }

        using var writer = FusionWriter.Create(EventActuatorData.Size);
        var data = EventActuatorData.Create(hashData, type, value);
        writer.Write(data);

        using var message = FusionMessage.ModuleCreate<EventActuatorMessage>(writer);
        MessageSender.BroadcastMessageExceptSelf(channel, message);

    }

    // TODO: Fix this causing crashes!
    // [HarmonyPatch(nameof(EventActuator.Actuate))]
    // [HarmonyPrefix]
    // public static bool ActuatePrefix(EventActuator __instance, double fixedTime, bool isInitializing)
    // {
    //     // No need for syncing if we aren't in a server
    //     if (!NetworkInfo.HasServer)
    //     {
    //         return true;
    //     }
    // 
    //     // Only allow the server to process event actuators
    //     if (NetworkInfo.IsServer)
    //     {
    //         return true;
    //     }
    // 
    //     // Otherwise, don't process them, have them be synced by the server
    //     return false;
    // }
}
