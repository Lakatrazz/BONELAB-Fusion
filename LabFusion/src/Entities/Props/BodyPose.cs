using LabFusion.Data;
using LabFusion.Math.Unity;
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
    }

    public void Interpolate(BodyPose from, BodyPose to, float t)
    {
        Position = Vector3.Lerp(from.Position, to.Position, t);
        Rotation = Quaternion.Slerp(from.Rotation, to.Rotation, t);
        Velocity = Vector3.Lerp(from.Velocity, to.Velocity, t);
        AngularVelocity = Vector3.Lerp(from.AngularVelocity, to.AngularVelocity, t);
    }

    public void Predict(float deltaTime) => PredictFrom(deltaTime, this);

    public void PredictFrom(float deltaTime, BodyPose reference)
    {
        Position += reference.Velocity * deltaTime;

        Rotation = UnityDerivatives.GetQuaternionDisplacement(deltaTime * reference.AngularVelocity) * Rotation;
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