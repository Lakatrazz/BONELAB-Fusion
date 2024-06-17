using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network;

public class GunShotData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public byte ammoCount;
    public ushort gunId;
    public byte gunIndex;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(ammoCount);
        writer.Write(gunId);
        writer.Write(gunIndex);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        ammoCount = reader.ReadByte();
        gunId = reader.ReadUInt16();
        gunIndex = reader.ReadByte();
    }

    public static GunShotData Create(byte smallId, byte ammoCount, ushort gunId, byte gunIndex)
    {
        return new GunShotData()
        {
            smallId = smallId,
            ammoCount = ammoCount,
            gunId = gunId,
            gunIndex = gunIndex,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class GunShotMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.GunShot;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<GunShotData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var gun = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.gunId);

        if (gun == null)
        {
            return;
        }

        var extender = gun.GetExtender<GunExtender>();

        if (extender == null)
        {
            return;
        }

        // Fire the gun, make sure it has ammo in its mag so it can fire properly
        var comp = extender.GetComponent(data.gunIndex);

        comp.hasFiredOnce = false;
        comp._hasFiredSinceLastBroadcast = false;
        comp.isTriggerPulledOnAttach = false;

        if (comp._magState != null)
        {
            comp._magState.SetCartridge(data.ammoCount + 1);
        }

        comp.CeaseFire();
        comp.Charge();

        if (!comp.allowFireOnSlideGrabbed)
            comp.SlideGrabbedReleased();

        GunPatches.IgnorePatches = true;
        comp.Fire();
        GunPatches.IgnorePatches = false;
    }
}