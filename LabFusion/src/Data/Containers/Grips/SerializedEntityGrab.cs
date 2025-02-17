using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Data;

public class EntityGrabGroupHandler : GrabGroupHandler<SerializedEntityGrab>
{
    public override GrabGroup? Group => GrabGroup.ENTITY;
}

public class SerializedEntityGrab : SerializedGrab
{
    public new const int Size = SerializedGrab.Size + sizeof(ushort) * 2;

    public ushort index;
    public ushort id;

    public SerializedEntityGrab() { }

    public SerializedEntityGrab(ushort index, ushort id)
    {
        this.index = index;
        this.id = id;
    }

    public override void Serialize(FusionWriter writer)
    {
        base.Serialize(writer);

        writer.Write(index);
        writer.Write(id);
    }

    public override void Deserialize(FusionReader reader)
    {
        base.Deserialize(reader);

        index = reader.ReadUInt16();
        id = reader.ReadUInt16();
    }

    public Grip GetGrip(out NetworkEntity entity)
    {
        entity = null;

        var foundEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(id);

        if (foundEntity != null)
        {
            entity = foundEntity;
            var gripExtender = foundEntity.GetExtender<GripExtender>();

            if (gripExtender != null)
            {
                return gripExtender.GetComponent(index);
            }
        }

        return null;
    }

    public override Grip GetGrip()
    {
        return GetGrip(out _);
    }
}