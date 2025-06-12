using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Messages;

public class MagazineInsertData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public int? GetSize() => Size;

    public ushort MagazineID;
    public ushort GunID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref MagazineID);
        serializer.SerializeValue(ref GunID);
    }
}

[Net.SkipHandleWhileLoading]
public class MagazineInsertMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineInsertData>();

        var mag = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.MagazineID);

        if (mag == null)
        {
            return;
        }

        var magExtender = mag.GetExtender<MagazineExtender>();

        if (magExtender == null)
        {
            return;
        }

        var gun = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.GunID);

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

                try
                {
                    if (otherPlug)
                    {
                        otherPlug.ForceEject();
                    }
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("ejecting other AmmoPlug", e);
                }

                AmmoSocketPatches.IgnorePatch = false;
            }
        }

        magExtender.Component.magazinePlug.host.TryDetach();

        AmmoSocketPatches.IgnorePatch = true;

        try
        {
            magExtender.Component.magazinePlug.InsertPlug(socketExtender.Component);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("inserting AmmoPlug", e);
        }

        AmmoSocketPatches.IgnorePatch = false;
    }
}