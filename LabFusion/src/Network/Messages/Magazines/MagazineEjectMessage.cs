using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class MagazineEjectData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 2;

    public byte smallId;
    public ushort magazineId;
    public ushort gunId;
    public Handedness hand;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref magazineId);
        serializer.SerializeValue(ref gunId);
        serializer.SerializeValue(ref hand, Precision.OneByte);
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
public class MagazineEjectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.MagazineEject;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineEjectData>();

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

            if (ammoPlug.magazine && MagazineExtender.Cache.TryGet(ammoPlug.magazine, out var magEntity) && magEntity.ID == data.magazineId)
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