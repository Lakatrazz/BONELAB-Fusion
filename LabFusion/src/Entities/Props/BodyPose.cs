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

    public Vector3 LinearPrediction = Vector3.zero;

    public Vector3 PredictedPosition => Position + LinearPrediction;

    public int? GetSize() => Size;

    public void ReadFrom(Rigidbody rigidbody)
    {
        Position = rigidbody.position;
        Rotation = rigidbody.rotation;
        Velocity = rigidbody.velocity;
        AngularVelocity = rigidbody.angularVelocity;
    }

    public void WriteTo(BodyPose target)
    {
        target.Position = Position;
        target.Rotation = Rotation;
        target.Velocity = Velocity;
        target.AngularVelocity = AngularVelocity;

        target.ResetPrediction();
    }

    public void Interpolate(BodyPose from, BodyPose to, float time)
    {
        Position = Vector3.Lerp(from.Position, to.Position, time);
        Rotation = Quaternion.Slerp(from.Rotation, to.Rotation, time);
        Velocity = Vector3.Lerp(from.Velocity, to.Velocity, time);
        AngularVelocity = Vector3.Lerp(from.AngularVelocity, to.AngularVelocity, time);
    }

    public void Predict(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public void ResetPrediction()
    {
        LinearPrediction = Vector3.zero;
    }

    public void PredictPositionOLDTEMP(float deltaTime)
    {
        LinearPrediction += Velocity * deltaTime;
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