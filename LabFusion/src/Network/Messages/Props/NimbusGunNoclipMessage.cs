using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public class NimbusGunNoclipData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public ushort syncId;
    public bool isEnabled;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(syncId);
        writer.Write(isEnabled);
    }

    public void Deserialize(FusionReader reader)
    {
        syncId = reader.ReadUInt16();
        isEnabled = reader.ReadBoolean();
    }

    public static NimbusGunNoclipData Create(ushort syncId, bool isEnabled)
    {
        return new NimbusGunNoclipData()
        {
            syncId = syncId,
            isEnabled = isEnabled,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class NimbusGunNoclipMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.NimbusGunNoclip;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<NimbusGunNoclipData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        if (received.Sender != entity.OwnerId.SmallId)
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