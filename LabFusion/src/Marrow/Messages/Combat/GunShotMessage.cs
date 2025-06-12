using LabFusion.Entities;
using LabFusion.Marrow.Patching;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow.Messages;

public class GunShotData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte ammoCount;
    public ushort gunId;
    public byte gunIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ammoCount);
        serializer.SerializeValue(ref gunId);
        serializer.SerializeValue(ref gunIndex);
    }

    public static GunShotData Create(byte ammoCount, ushort gunId, byte gunIndex)
    {
        return new GunShotData()
        {
            ammoCount = ammoCount,
            gunId = gunId,
            gunIndex = gunIndex,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class GunShotMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GunShotData>();

        var gun = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.gunId);

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