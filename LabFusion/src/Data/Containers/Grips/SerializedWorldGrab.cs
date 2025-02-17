using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

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

    public override void Serialize(FusionWriter writer)
    {
        base.Serialize(writer);

        writer.Write(grabberId);
    }

    public override void Deserialize(FusionReader reader)
    {
        base.Deserialize(reader);

        grabberId = reader.ReadByte();
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