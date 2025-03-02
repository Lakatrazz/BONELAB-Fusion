using LabFusion.Bonelab.Extenders;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab;

public enum SimpleGripEventType
{
    TRIGGER_DOWN = 0,
    MENU_TAP = 1,
    ATTACH = 2,
    DETACH = 3,
}

public class SimpleGripEventData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public ushort entityId;
    public byte gripEventIndex;
    public SimpleGripEventType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref gripEventIndex);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static SimpleGripEventData Create(ushort entityId, byte gripEventIndex, SimpleGripEventType type)
    {
        return new SimpleGripEventData()
        {
            entityId = entityId,
            gripEventIndex = gripEventIndex,
            type = type
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SimpleGripEventMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SimpleGripEventData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

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