using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.Data;

public class SerializedLocalTransform : IFusionSerializable
{
    public const int Size = SerializedShortVector3.Size + SerializedSmallQuaternion.Size;
    public static readonly SerializedLocalTransform Default = new(Vector3Extensions.zero, QuaternionExtensions.identity);

    public Vector3 position;
    public Quaternion rotation;

    private SerializedShortVector3 _compressedPosition;
    private SerializedSmallQuaternion _compressedRotation;

    public void Serialize(FusionWriter writer)
    {
        _compressedPosition = SerializedShortVector3.Compress(position);
        _compressedRotation = SerializedSmallQuaternion.Compress(rotation);

        writer.Write(_compressedPosition);
        writer.Write(_compressedRotation);
    }

    public void Deserialize(FusionReader reader)
    {
        _compressedPosition = reader.ReadFusionSerializable<SerializedShortVector3>();
        position = _compressedPosition.Expand();

        _compressedRotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
        rotation = _compressedRotation.Expand();
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