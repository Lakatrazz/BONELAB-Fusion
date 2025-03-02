using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Scene;
using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class SerializedTransform : INetSerializable
{
    public const ushort Size = sizeof(float) * 3 + SerializedQuaternion.Size;
    public static readonly SerializedTransform Default = new(Vector3Extensions.zero, QuaternionExtensions.identity);

    public Vector3 position;
    public Quaternion rotation;

    private SerializedQuaternion _compressedRotation;

    public void Serialize(INetSerializer serializer)
    {
        var position = this.position;

        if (!serializer.IsReader)
        {
            position = NetworkTransformManager.EncodePosition(position);
        }

        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref _compressedRotation);

        if (serializer.IsReader)
        {
            this.position = NetworkTransformManager.DecodePosition(position);
            this.rotation = _compressedRotation.Expand();
        }
    }

    public SerializedTransform() { }

    public SerializedTransform(Transform transform)
        : this(transform.position, transform.rotation) { }

    public SerializedTransform(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;

        this._compressedRotation = SerializedQuaternion.Compress(this.rotation);
    }
}
