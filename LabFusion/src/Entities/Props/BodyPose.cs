using LabFusion.Data;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Entities;

public class BodyPose : IFusionSerializable
{
    public const int Size = SerializedShortVector3.Size + SerializedSmallQuaternion.Size + SerializedSmallVector3.Size * 2;

    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    public Vector3 velocity = Vector3.zero;
    public Vector3 angularVelocity = Vector3.zero;

    private Vector3 _positionPrediction = Vector3.zero;

    public Vector3 PredictedPosition => position + _positionPrediction;

    public void ReadFrom(Rigidbody rigidbody)
    {
        position = rigidbody.position;
        rotation = rigidbody.rotation;
        velocity = rigidbody.velocity;
        angularVelocity = rigidbody.angularVelocity;
    }

    public void CopyTo(BodyPose target)
    {
        target.position = position;
        target.rotation = rotation;
        target.velocity = velocity;
        target.angularVelocity = angularVelocity;

        target.ResetPrediction();
    }

    public void ResetPrediction()
    {
        _positionPrediction = Vector3.zero;
    }

    public void PredictPosition(float deltaTime)
    {
        _positionPrediction += velocity * deltaTime;
    }

    public int? GetSize()
    {
        return Size;
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(SerializedShortVector3.Compress(position));
        writer.Write(SerializedSmallQuaternion.Compress(rotation));
        writer.Write(SerializedSmallVector3.Compress(velocity));
        writer.Write(SerializedSmallVector3.Compress(angularVelocity));
    }

    public void Deserialize(FusionReader reader)
    {
        position = reader.ReadFusionSerializable<SerializedShortVector3>().Expand();
        rotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>().Expand();
        velocity = reader.ReadFusionSerializable<SerializedSmallVector3>().Expand();
        angularVelocity = reader.ReadFusionSerializable<SerializedSmallVector3>().Expand();
    }
}