using LabFusion.Network.Serialization;

namespace LabFusion.Entities;

public class EntityPose : INetSerializable   
{
    public BodyPose[] bodies;

    public EntityPose() { }

    public EntityPose(int bodyCount)
    {
        bodies = new BodyPose[bodyCount];

        for (var i = 0; i < bodyCount; i++)
        {
            bodies[i] = new BodyPose();
        }
    }

    public void CopyTo(EntityPose target)
    {
        if (target.bodies.Length != bodies.Length) 
        {
            return;
        }

        for (var i = 0; i < target.bodies.Length; i++)
        {
            bodies[i].CopyTo(target.bodies[i]);
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        if (serializer is NetWriter writer)
        {
            Serialize(writer);
        }
        else if (serializer is NetReader reader)
        {
            Deserialize(reader);
        }
    }

    public void Serialize(NetWriter writer)
    {
        byte length = (byte)bodies.Length;

        writer.Write(length);

        for (var i = 0; i < length; i++)
        {
            writer.SerializeValue(ref bodies[i]);
        }
    }

    public void Deserialize(NetReader reader)
    {
        byte length = reader.ReadByte();

        bodies = new BodyPose[length];

        for (var i = 0; i < length; i++)
        {
            reader.SerializeValue(ref bodies[i]);
        }
    }
}