using LabFusion.Marrow.Patching;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class GunShotData : INetSerializable
{
    public const int Size = sizeof(byte) + ComponentIndexData.Size;

    public byte AmmoCount;

    public ComponentIndexData Gun;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref AmmoCount);
        serializer.SerializeValue(ref Gun);
    }
}

[Net.SkipHandleWhileLoading]
public class GunShotMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GunShotData>();

        var gunEntity = data.Gun.Entity.GetEntity();

        if (gunEntity == null)
        {
            return;
        }

        var gunExtender = gunEntity.GetExtender<GunExtender>();

        if (gunExtender == null)
        {
            return;
        }

        // Fire the gun, make sure it has ammo in its mag so it can fire properly
        var gun = gunExtender.GetComponent(data.Gun.ComponentIndex);

        gun.hasFiredOnce = false;
        gun.isTriggerPulledOnAttach = false;

        var magState = gun.MagazineState;
        bool hasMagState = magState != null;

        // If there is a magazine, we can modify the ammo count
        if (hasMagState)
        {
            magState.SetCartridge(data.AmmoCount + 1);
        }

        gun.CeaseFire();
        gun.Charge();

        // If no magazine is available, we still want the gun to fire
        // So we can forcefully insert the default cartridge
        if (!hasMagState)
        {
            gun.chamberedCartridge = gun.defaultCartridge;
            gun.cartridgeState = Gun.CartridgeStates.UNSPENT;
            gun.isCharged = true;
        }

        if (!gun.allowFireOnSlideGrabbed)
        {
            gun.SlideGrabbedReleased();
        }

        GunPatches.IgnorePatches = true;
        gun.Fire();
        GunPatches.IgnorePatches = false;
    }
}