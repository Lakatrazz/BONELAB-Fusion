using LabFusion.Data;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Patching;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Circuits;

using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Messages;

public enum EventActuatorType : byte
{
    ROSE,
    FELL,
    ROSEONESHOT,
}

public class EventActuatorData : INetSerializable
{
    public const int Size = ComponentHashData.Size + sizeof(byte) * 2 + sizeof(float);

    public byte playerId;

    public ComponentHashData hashData;

    public EventActuatorType type;

    public float value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref hashData);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref value);
    }

    public static EventActuatorData Create(byte playerId, ComponentHashData hashData, EventActuatorType type, float value)
    {
        return new EventActuatorData()
        {
            playerId = playerId,
            hashData = hashData,
            type = type,
            value = value,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class EventActuatorMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EventActuatorData>();

        var eventActuator = EventActuatorPatches.HashTable.GetComponentFromData(data.hashData);

        if (eventActuator == null)
        {
            return;
        }

        OnFoundEventActuator(eventActuator, data.type, data.value);
    }

    private static void OnFoundEventActuator(EventActuator actuator, EventActuatorType type, float value)
    {

        EventActuatorPatches.IgnoreOverride = true;

        try
        {
            switch (type)
            {
                case EventActuatorType.ROSE:
                    actuator._invokeInputRose(value);
                    break;
                case EventActuatorType.FELL:
                    actuator._invokeInputFell(value);
                    break;
                case EventActuatorType.ROSEONESHOT:
                    actuator._invokeInputRoseOneShot(value);
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"executing EventActuator {type}", e);
        }

        EventActuatorPatches.IgnoreOverride = false;
    }
}