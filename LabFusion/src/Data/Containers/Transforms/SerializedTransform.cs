using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.Data
{
    public class SerializedTransform : IFusionSerializable
    {
        public const ushort Size = sizeof(float) * 3 + SerializedQuaternion.Size;
        public static readonly SerializedTransform Default = new(Vector3Extensions.zero, QuaternionExtensions.identity);

        public Vector3 position;
        public Quaternion rotation;

        private SerializedQuaternion _compressedRotation;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(position);
            writer.Write(_compressedRotation);
        }

        public void Deserialize(FusionReader reader)
        {
            position = reader.ReadVector3();

            _compressedRotation = reader.ReadFusionSerializable<SerializedQuaternion>();
            rotation = _compressedRotation.Expand();
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
}
