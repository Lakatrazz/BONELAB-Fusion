using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;
using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class WorldGrabGroupHandler : GrabGroupHandler<SerializedWorldGrab>
{
    public override GrabGroup? Group => GrabGroup.WORLD;
}

public class SerializedWorldGrab : SerializedGrab
{
    public new const int Size = SerializedGrab.Size + sizeof(byte);

    public byte grabberId;

    public SerializedWorldGrab() { }

    public SerializedWorldGrab(byte grabberId)
    {
        this.grabberId = grabberId;
    }

    public override int GetSize()
    {
        return Size;
    }

    public override void Serialize(INetSerializer serializer)
    {
        base.Serialize(serializer);

        serializer.SerializeValue(ref grabberId);
    }

    public override Grip GetGrip()
    {
        if (NetworkPlayerManager.TryGetPlayer(grabberId, out var player) && player.HasRig)
        {
            var worldGrip = player.RigRefs.RigManager.worldGrip;
            return worldGrip;
        }

        return null;
    }
}