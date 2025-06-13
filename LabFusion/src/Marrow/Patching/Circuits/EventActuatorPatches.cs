using HarmonyLib;

using Il2CppSLZ.Marrow.Circuits;
using Il2CppUltEvents;

using LabFusion.Data;
using LabFusion.Marrow.Circuits;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;
using LabFusion.Marrow.Messages;

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
        // I wish this game was mono...
        OverrideEvent(actuator, actuator.InputRose, OnInputRose);
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

        var copiedEvent = new UltEvent<float>
        {
            _PersistentCalls = new Il2CppSystem.Collections.Generic.List<PersistentCall>()
        };

        foreach (var call in originalCalls)
        {
            copiedEvent.PersistentCallsList.Add(call);
        }

        ultEvent.Clear();

        ultEvent.add_DynamicCalls((Il2CppSystem.Action<float>)NewCall);

        void NewCall(float parameter0)
        {
            if (!NetworkSceneManager.IsLevelNetworked || IgnoreOverride)
            {
                RunOriginal(parameter0);
                return;
            }

            // Check owner
            bool isOwner = CheckOwnership(actuator);

            if (isOwner)
            {
                replacement(actuator, parameter0);
            }
        }

        void RunOriginal(float parameter0)
        {
            copiedEvent.Invoke(parameter0);
        }
    }

    private static bool CheckOwnership(EventActuator actuator)
    {
        var input = actuator.input;

        if (input == null)
        {
            return NetworkSceneManager.IsLevelHost;
        }

        var networkEntity = CircuitHelper.GetNetworkEntity(input);

        if (networkEntity != null)
        {
            return networkEntity.IsOwner;
        }

        return NetworkSceneManager.IsLevelHost;
    }

    private static void OnInputRose(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.ROSE, f);
    }

    private static void OnInputFell(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.FELL, f);
    }

    private static void OnInputRoseOneShot(EventActuator actuator, float f)
    {
        OnSendEvent(actuator, EventActuatorType.ROSEONESHOT, f);
    }

    private static void OnSendEvent(EventActuator actuator, EventActuatorType type, float value)
    {
        var hashData = HashTable.GetDataFromComponent(actuator);

        if (hashData == null)
        {
            return;
        }

        var data = EventActuatorData.Create(PlayerIDManager.LocalSmallID, hashData, type, value);

        MessageRelay.RelayModule<EventActuatorMessage, EventActuatorData>(data, CommonMessageRoutes.ReliableToClients);
    }
}
