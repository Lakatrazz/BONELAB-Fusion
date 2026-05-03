using LabFusion.Network.Serialization;

namespace LabFusion.Entities;

public class EntityPose : INetSerializable   
{
    public int BodyCount => Bodies.Length;

    public BodyPose[] Bodies { get; private set; } = Array.Empty<BodyPose>();

    public int? GetSize() => sizeof(byte) + BodyPose.Size * BodyCount;

    public EntityPose() { }

    public EntityPose(int bodyCount)
    {
        Bodies = new BodyPose[bodyCount];

        for (var i = 0; i < bodyCount; i++)
        {
            Bodies[i] = new BodyPose();
        }
    }

    public void WriteTo(EntityPose target)
    {
        if (target.BodyCount != BodyCount) 
        {
            return;
        }

        for (var i = 0; i < target.BodyCount; i++)
        {
            Bodies[i].WriteTo(target.Bodies[i]);
        }
    }

    public void PredictOLDTEMP(float deltaTime)
    {
        foreach (var body in Bodies)
        {
            body.PredictPositionOLDTEMP(deltaTime);
        }
    }

    public void ResetPrediction()
    {
        foreach (var body in Bodies)
        {
            body.ResetPrediction();
        }
    }

    public void Interpolate(EntityPose from, EntityPose to, float t)
    {
        for (var i = 0; i < BodyCount; i++)
        {
            Bodies[i].Interpolate(from.Bodies[i], to.Bodies[i], t);
        }
    }

    public void Predict(float deltaTime)
    {
        for (var i = 0; i < BodyCount; i++)
        {
            Bodies[i].Predict(deltaTime);
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
        byte length = (byte)BodyCount;

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