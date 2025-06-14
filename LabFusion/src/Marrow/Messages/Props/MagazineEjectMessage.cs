using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Marrow.Messages;

public class MagazineEjectData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 2;

    public int? GetSize() => Size;

    public byte PlayerID;
    public ushort MagazineID;
    public ushort GunID;
    public Handedness Handedness;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref MagazineID);
        serializer.SerializeValue(ref GunID);
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class MagazineEjectMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineEjectData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.GunID);

        if (entity == null)
        {
            return;
        }

        var ammoSocketExtender = entity.GetExtender<AmmoSocketExtender>();

        if (ammoSocketExtender == null)
        {
            return;
        }

        var ammoSocket = ammoSocketExtender.Component;

        if (!ammoSocket._magazinePlug)
        {
            return;
        }

        var ammoPlug = ammoSocket._magazinePlug;

        if (!ammoPlug.magazine)
        {
            return;
        }

        var magEntity = MagazineExtender.Cache.Get(ammoPlug.magazine);

        if (magEntity == null || magEntity.ID != data.MagazineID)
        {
            return;
        }

        AmmoSocketPatches.IgnorePatch = true;

        try
        {
            ammoPlug.ForceEject();

            if (data.Handedness != Handedness.UNDEFINED && NetworkPlayerManager.TryGetPlayer(data.PlayerID, out var player) && !player.NetworkEntity.IsOwner)
            {
                player.Grabber.Attach(data.Handedness, ammoPlug.magazine.grip);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("ejecting AmmoPlug", e);
        }

        AmmoSocketPatches.IgnorePatch = false;
    }
}