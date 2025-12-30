using LabFusion.Network.Serialization;

namespace LabFusion.Entities;

public class EntityPose : INetSerializable   
{
    public BodyPose[] Bodies;

    public int? GetSize() => sizeof(byte) + BodyPose.Size * Bodies.Length;

    public EntityPose() { }

    public EntityPose(int bodyCount)
    {
        Bodies = new BodyPose[bodyCount];

        for (var i = 0; i < bodyCount; i++)
        {
            Bodies[i] = new BodyPose();
        }
    }

    public void CopyTo(EntityPose target)
    {
        if (target.Bodies.Length != Bodies.Length) 
        {
            return;
        }

        for (var i = 0; i < target.Bodies.Length; i++)
        {
            Bodies[i].CopyTo(target.Bodies[i]);
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
        byte length = (byte)Bodies.Length;

        writer.Write(length);

        for (var i = 0; i < length; i++)
        {
            writer.SerializeValue(ref Bodies[i]);
        }
    }

    public void Deserialize(NetReader reader)
    {
        byte length = reader.ReadByte();

        Bodies = new BodyPose[length];

        for (var i = 0; i < length; i++)
        {
            reader.SerializeValue(ref Bodies[i]);
        }
    }
}