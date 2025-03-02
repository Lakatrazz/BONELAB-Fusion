using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class SerializedLocalTransform : INetSerializable
{
    public const int Size = SerializedShortVector3.Size + SerializedSmallQuaternion.Size;
    public static readonly SerializedLocalTransform Default = new(Vector3Extensions.zero, QuaternionExtensions.identity);

    public Vector3 position;
    public Quaternion rotation;

    private SerializedShortVector3 _compressedPosition;
    private SerializedSmallQuaternion _compressedRotation;

    public void Serialize(INetSerializer serializer)
    {
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref _compressedPosition);
            serializer.SerializeValue(ref _compressedRotation);

            position = _compressedPosition.Expand();
            rotation = _compressedRotation.Expand();
        }
        else
        {
            _compressedPosition = SerializedShortVector3.Compress(position);
            _compressedRotation = SerializedSmallQuaternion.Compress(rotation);

            serializer.SerializeValue(ref _compressedPosition);
            serializer.SerializeValue(ref _compressedRotation);
        }
    }

    public SerializedLocalTransform() { }

    public SerializedLocalTransform(Transform transform)
        : this(transform.localPosition, transform.localRotation) { }

    public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
    {
        this.position = localPosition;
        this.rotation = localRotation;
    }
}