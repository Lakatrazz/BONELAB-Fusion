using LabFusion.Data;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Entities;

public class BodyPose : INetSerializable
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

    public void Serialize(INetSerializer serializer)
    {
        SerializedShortVector3 position = null;
        SerializedSmallQuaternion rotation = null;
        SerializedSmallVector3 velocity = null;
        SerializedSmallVector3 angularVelocity = null;

        if (!serializer.IsReader)
        {
            position = SerializedShortVector3.Compress(this.position);
            rotation = SerializedSmallQuaternion.Compress(this.rotation);
            velocity = SerializedSmallVector3.Compress(this.velocity);
            angularVelocity = SerializedSmallVector3.Compress(this.angularVelocity);
        }

        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);

        if (serializer.IsReader)
        {
            this.position = position.Expand();
            this.rotation = rotation.Expand();
            this.velocity = velocity.Expand();
            this.angularVelocity = angularVelocity.Expand();
        }
    }
}