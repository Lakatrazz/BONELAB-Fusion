using LabFusion.Data;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Entities;

public class BodyPose : INetSerializable
{
    public const int Size = SerializedShortVector3.Size + SerializedSmallQuaternion.Size + SerializedSmallVector3.Size * 2;

    public Vector3 Position = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;

    public Vector3 Velocity = Vector3.zero;
    public Vector3 AngularVelocity = Vector3.zero;

    private Vector3 _positionPrediction = Vector3.zero;

    public Vector3 PredictedPosition => Position + _positionPrediction;

    public int? GetSize() => Size;

    public void ReadFrom(Rigidbody rigidbody)
    {
        Position = rigidbody.position;
        Rotation = rigidbody.rotation;
        Velocity = rigidbody.velocity;
        AngularVelocity = rigidbody.angularVelocity;
    }

    public void CopyTo(BodyPose target)
    {
        target.Position = Position;
        target.Rotation = Rotation;
        target.Velocity = Velocity;
        target.AngularVelocity = AngularVelocity;

        target.ResetPrediction();
    }

    public void ResetPrediction()
    {
        _positionPrediction = Vector3.zero;
    }

    public void PredictPosition(float deltaTime)
    {
        _positionPrediction += Velocity * deltaTime;
    }

    public void Serialize(INetSerializer serializer)
    {
        SerializedShortVector3 position = null;
        SerializedSmallQuaternion rotation = null;
        SerializedSmallVector3 velocity = null;
        SerializedSmallVector3 angularVelocity = null;

        if (!serializer.IsReader)
        {
            position = SerializedShortVector3.Compress(this.Position);
            rotation = SerializedSmallQuaternion.Compress(this.Rotation);
            velocity = SerializedSmallVector3.Compress(this.Velocity);
            angularVelocity = SerializedSmallVector3.Compress(this.AngularVelocity);
        }

        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);

        if (serializer.IsReader)
        {
            this.Position = position.Expand();
            this.Rotation = rotation.Expand();
            this.Velocity = velocity.Expand();
            this.AngularVelocity = angularVelocity.Expand();
        }
    }
}