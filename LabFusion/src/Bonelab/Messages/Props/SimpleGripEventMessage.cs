using LabFusion.Bonelab.Extenders;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab.Messages;

public enum SimpleGripEventType
{
    TRIGGER_DOWN = 0,
    MENU_TAP = 1,
    ATTACH = 2,
    DETACH = 3,
}

public class SimpleGripEventData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(byte) * 2;

    public int? GetSize() => Size;

    public NetworkEntityReference Entity;
    public byte GripEventIndex;
    public SimpleGripEventType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
        serializer.SerializeValue(ref GripEventIndex);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class SimpleGripEventMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SimpleGripEventData>();

        if (!data.Entity.TryGetEntity(out var entity))
        {
            return;
        }

        var extender = entity.GetExtender<SimpleGripEventsExtender>();

        if (extender == null)
        {
            return;
        }

        var gripEvent = extender.GetComponent(data.GripEventIndex);

        if (gripEvent == null)
        {
            return;
        }

        try
        {
            switch (data.Type)
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
        catch { }
    }
}