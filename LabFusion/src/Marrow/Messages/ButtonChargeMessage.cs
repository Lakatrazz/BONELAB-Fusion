using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow;

public class ButtonChargeData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort entityId;

    public bool charged;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(entityId);

        writer.Write(charged);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        entityId = reader.ReadUInt16();

        charged = reader.ReadBoolean();
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
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ButtonChargeData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.ModuleCreate<ButtonChargeMessage>(bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

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