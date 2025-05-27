using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;

namespace LabFusion.Marrow.Messages;

public class MagazineInsertData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public int? GetSize() => Size;

    public ushort MagazineId;
    public ushort GunId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref MagazineId);
        serializer.SerializeValue(ref GunId);
    }
}

[Net.DelayWhileTargetLoading]
public class MagazineInsertMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineInsertData>();

        var mag = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.MagazineId);

        if (mag == null)
        {
            return;
        }

        var magExtender = mag.GetExtender<MagazineExtender>();

        if (magExtender == null)
        {
            return;
        }

        var gun = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.GunId);

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