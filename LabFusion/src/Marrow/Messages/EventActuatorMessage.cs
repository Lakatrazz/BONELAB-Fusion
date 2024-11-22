using LabFusion.Data;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Patching;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Circuits;

namespace LabFusion.Marrow;

public enum EventActuatorType : byte
{
    ROSE,
    FELL,
    ROSEONESHOT,
}

public class EventActuatorData : IFusionSerializable
{
    public const int Size = ComponentHashData.Size + sizeof(byte) * 2 + sizeof(float);

    public byte playerId;

    public ComponentHashData hashData;

    public EventActuatorType type;

    public float value;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(playerId);

        writer.Write(hashData);

        writer.Write((byte)type);

        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        playerId = reader.ReadByte();

        hashData = reader.ReadFusionSerializable<ComponentHashData>();

        type = (EventActuatorType)reader.ReadByte();

        value = reader.ReadSingle();
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
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EventActuatorData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.ModuleCreate<EventActuatorMessage>(bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);

            return;
        }

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