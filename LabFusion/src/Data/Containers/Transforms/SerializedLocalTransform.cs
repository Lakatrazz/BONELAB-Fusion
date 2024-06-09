using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.Data
{
    public class SerializedLocalTransform : IFusionSerializable
    {
        public const int Size = sizeof(float) * 3 + SerializedSmallQuaternion.Size;
        public static readonly SerializedLocalTransform Default = new(Vector3Extensions.zero, QuaternionExtensions.identity);

        public Vector3 position;
        public Quaternion rotation;

        private SerializedSmallQuaternion _compressedRotation;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(position);
            writer.Write(_compressedRotation);
        }

        public void Deserialize(FusionReader reader)
        {
            position = reader.ReadVector3();

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

            this._compressedRotation = SerializedSmallQuaternion.Compress(this.rotation);
        }
    }
}
