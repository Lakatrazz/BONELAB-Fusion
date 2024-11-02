using HarmonyLib;

using Il2CppSLZ.Marrow.Circuits;
using Il2CppUltEvents;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(EventActuator))]
public static class EventActuatorPatches
{
    public static readonly ComponentHashTable<EventActuator> HashTable = new();

    public static bool IgnoreOverride { get; set; } = false;

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

        // Override the UltEvents
        // Unfortunately, interop completely fails to patch EventActuator.Actuate and crashes when it runs
        // So, I have to do this mess and instead replace the UltEvents
        OverrideEvent(actuator, actuator.InputUpdated, OnInputUpdated);
        OverrideEvent(actuator, actuator.InputRose, OnInputRose);
        OverrideEvent(actuator, actuator.InputHeld, OnInputHeld);
        OverrideEvent(actuator, actuator.InputFell, OnInputFell);
        OverrideEvent(actuator, actuator.InputRoseOneShot, OnInputRoseOneShot);
    }

    public static void OnDestroy(EventActuator actuator)
    {
        HashTable.RemoveComponent(actuator);
    }

    private static void OverrideEvent(EventActuator actuator, UltEvent<float> ultEvent, Action<EventActuator, float> replacement)
    {
        // No calls? No need to override
        if (!ultEvent.HasCalls)
        {
            return;
        }

        var originalCalls = ultEvent.PersistentCallsList;

        var copiedEvent = new UltEvent<float>();
        copiedEvent._PersistentCalls = new Il2CppSystem.Collections.Generic.List<PersistentCall>();

        foreach (var call in originalCalls)
        {
            copiedEvent.PersistentCallsList.Add(call);
        }

        ultEvent.Clear();

        ultEvent.add_DynamicCalls((Il2CppSystem.Action<float>)NewCall);

        void NewCall(float parameter0)
        {
            if (!NetworkInfo.HasServer || IgnoreOverride)
            {
                RunOriginal(parameter0);
                return;
            }

            if (NetworkInfo.IsServer)
            {
                replacement(actuator, parameter0);
                RunOriginal(parameter0);
                return;
            }
        }

        void RunOriginal(float parameter0)
        {
            copiedEvent.Invoke(parameter0);
        }
    }

    private static void OnInputUpdated(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.UPDATED, f, NetworkChannel.Unreliable);
    }

    private static void OnInputRose(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.ROSE, f, NetworkChannel.Reliable);
    }

    private static void OnInputHeld(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.HELD, f, NetworkChannel.Unreliable);
    }

    private static void OnInputFell(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.FELL, f, NetworkChannel.Reliable);
    }

    private static void OnInputRoseOneShot(EventActuator actuator, float f)
    {
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
}
