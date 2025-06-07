using LabFusion.Network;
using LabFusion.Marrow.Extenders;
using LabFusion.SDK.Modules;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Messages;

public class NimbusGunNoClipData : INetSerializable
{
    public const int Size = sizeof(ushort) + sizeof(bool);

    public int? GetSize() => Size;

    public ushort NimbusGunID;
    public bool NoClip;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NimbusGunID);
        serializer.SerializeValue(ref NoClip);
    }
}

[Net.SkipHandleWhileLoading]
public class NimbusGunNoClipMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<NimbusGunNoClipData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.NimbusGunID);

        if (entity == null)
        {
            return;
        }

        if (received.Sender != entity.OwnerID.SmallID)
        {
            return;
        }

        var extender = entity.GetExtender<FlyingGunExtender>();

        if (extender == null)
        {
            return;
        }

        var nimbus = extender.Component;

        if (data.NoClip)
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