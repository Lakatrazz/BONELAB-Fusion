using LabFusion.Bonelab.Extenders;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab;

public enum SimpleGripEventType
{
    TRIGGER_DOWN = 0,
    MENU_TAP = 1,
    ATTACH = 2,
    DETACH = 3,
}

public class SimpleGripEventData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public byte gripEventIndex;
    public SimpleGripEventType type;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(syncId);
        writer.Write(gripEventIndex);
        writer.Write((byte)type);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        syncId = reader.ReadUInt16();
        gripEventIndex = reader.ReadByte();
        type = (SimpleGripEventType)reader.ReadByte();
    }

    public static SimpleGripEventData Create(byte smallId, ushort syncId, byte gripEventIndex, SimpleGripEventType type)
    {
        return new SimpleGripEventData()
        {
            smallId = smallId,
            syncId = syncId,
            gripEventIndex = gripEventIndex,
            type = type
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SimpleGripEventMessage : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<SimpleGripEventData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.ModuleCreate<SimpleGripEventMessage>(bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<SimpleGripEventsExtender>();

        if (extender == null)
        {
            return;
        }

        var gripEvent = extender.GetComponent(data.gripEventIndex);

        if (gripEvent == null)
        {
            return;
        }

        switch (data.type)
        {
            default:
            case SimpleGripEventType.TRIGGER_DOWN:
                gripEvent.OnIndexDown.Invoke();
                break;
            case SimpleGripEventType.MENU_TAP:
                gripEvent.OnMenuTapDown.Invoke();
                break;
            case SimpleGripEventType.ATTACH:
                gripEvent.OnAttach.Invoke();
                break;
            case SimpleGripEventType.DETACH:
                gripEvent.OnDetach.Invoke();
                break;
        }
    }
}