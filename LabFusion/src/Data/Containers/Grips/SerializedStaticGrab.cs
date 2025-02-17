using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Patching;

using Il2CppSLZ.Marrow;

namespace LabFusion.Data;

public class StaticGrabGroupHandler : GrabGroupHandler<SerializedStaticGrab>
{
    public override GrabGroup? Group => GrabGroup.STATIC;
}

public class SerializedStaticGrab : SerializedGrab
{
    public new const int Size = SerializedGrab.Size + ComponentHashData.Size;

    public ComponentHashData gripHash = null;

    public SerializedStaticGrab() { }

    public SerializedStaticGrab(ComponentHashData gripHash)
    {
        this.gripHash = gripHash;
    }

    public override int GetSize()
    {
        return Size;
    }

    public override void Serialize(FusionWriter writer)
    {
        base.Serialize(writer);

        writer.Write(gripHash);
    }

    public override void Deserialize(FusionReader reader)
    {
        base.Deserialize(reader);

        gripHash = reader.ReadFusionSerializable<ComponentHashData>();
    }

    public override Grip GetGrip()
    {
        var grip = GripPatches.HashTable.GetComponentFromData(gripHash);

        return grip;
    }
}