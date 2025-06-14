using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;
using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Messages;

public class ButtonChargeData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort entityId;

    public bool charged;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref charged);
    }

    public static ButtonChargeData Create(byte smallId, ushort entityId, bool charged)
    {
        return new ButtonChargeData()
        {
            smallId = smallId,
            entityId = entityId,
            charged = charged,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ButtonChargeMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ButtonChargeData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<ButtonControllerExtender>();

        if (extender == null)
        {
            return;
        }

        extender.Charged = data.charged;
    }
}