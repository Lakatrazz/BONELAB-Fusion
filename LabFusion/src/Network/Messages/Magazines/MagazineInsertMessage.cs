using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class MagazineInsertData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 2;

    public byte smallId;
    public ushort magazineId;
    public ushort gunId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref magazineId);
        serializer.SerializeValue(ref gunId);
    }

    public static MagazineInsertData Create(byte smallId, ushort magazineId, ushort gunId)
    {
        return new MagazineInsertData()
        {
            smallId = smallId,
            magazineId = magazineId,
            gunId = gunId,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class MagazineInsertMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.MagazineInsert;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineInsertData>();

        var mag = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.magazineId);

        if (mag == null)
        {
            return;
        }

        var magExtender = mag.GetExtender<MagazineExtender>();

        if (magExtender == null)
        {
            return;
        }

        var gun = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.gunId);

        if (gun == null)
        {
            return;
        }

        var socketExtender = gun.GetExtender<AmmoSocketExtender>();

        if (socketExtender == null)
        {
            return;
        }

        // Insert mag into gun
        if (socketExtender.Component._magazinePlug)
        {
            var otherPlug = socketExtender.Component._magazinePlug;

            if (otherPlug != magExtender.Component.magazinePlug)
            {
                AmmoSocketPatches.IgnorePatch = true;

                if (otherPlug)
                {
                    otherPlug.ForceEject();
                }

                AmmoSocketPatches.IgnorePatch = false;
            }
        }

        magExtender.Component.magazinePlug.host.TryDetach();

        AmmoSocketPatches.IgnorePatch = true;

        magExtender.Component.magazinePlug.InsertPlug(socketExtender.Component);

        AmmoSocketPatches.IgnorePatch = false;
    }
}