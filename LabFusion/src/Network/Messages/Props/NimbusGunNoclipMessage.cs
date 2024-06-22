using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class NimbusGunNoclipData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public bool isEnabled;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(syncId);
        writer.Write(isEnabled);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        syncId = reader.ReadUInt16();
        isEnabled = reader.ReadBoolean();
    }

    public static NimbusGunNoclipData Create(byte smallId, ushort syncId, bool isEnabled)
    {
        return new NimbusGunNoclipData()
        {
            smallId = smallId,
            syncId = syncId,
            isEnabled = isEnabled,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class NimbusGunNoclipMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.NimbusGunNoclip;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<NimbusGunNoclipData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<FlyingGunExtender>();

        if (extender == null)
        {
            return;
        }

        var nimbus = extender.Component;

        if (data.isEnabled)
        {
            nimbus.EnableNoClip();
            nimbus.sfx.Release();
        }
        else if (nimbus.triggerGrip)
        {
            var hand = nimbus.triggerGrip.GetHand();

            if (hand)
            {
                nimbus.DisableNoClip(hand);
                nimbus.sfx.Grab();
            }
        }
    }
}