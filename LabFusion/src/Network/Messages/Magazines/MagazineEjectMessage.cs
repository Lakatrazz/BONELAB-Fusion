using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network;

public class MagazineEjectData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 2;

    public byte smallId;
    public ushort magazineId;
    public ushort gunId;
    public Handedness hand;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(magazineId);
        writer.Write(gunId);
        writer.Write((byte)hand);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        magazineId = reader.ReadUInt16();
        gunId = reader.ReadUInt16();
        hand = (Handedness)reader.ReadByte();
    }

    public static MagazineEjectData Create(byte smallId, ushort magazineId, ushort gunId, Handedness hand)
    {
        return new MagazineEjectData()
        {
            smallId = smallId,
            magazineId = magazineId,
            gunId = gunId,
            hand = hand,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class MagazineEjectMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.MagazineEject;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<MagazineEjectData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.gunId);

        if (entity == null)
        {
            return;
        }

        var ammoSocketExtender = entity.GetExtender<AmmoSocketExtender>();

        if (ammoSocketExtender == null)
        {
            return;
        }

        // Eject mag from gun
        if (ammoSocketExtender.Component._magazinePlug)
        {
            AmmoSocketPatches.IgnorePatch = true;

            var ammoPlug = ammoSocketExtender.Component._magazinePlug;

            if (ammoPlug.magazine && MagazineExtender.Cache.TryGet(ammoPlug.magazine, out var magEntity) && magEntity.Id == data.magazineId)
            {
                ammoPlug.ForceEject();

                if (data.hand != Handedness.UNDEFINED && NetworkPlayerManager.TryGetPlayer(data.smallId, out var player) && !player.NetworkEntity.IsOwner)
                {
                    player.Grabber.Attach(data.hand, ammoPlug.magazine.grip);
                }
            }

            AmmoSocketPatches.IgnorePatch = false;
        }
    }
}