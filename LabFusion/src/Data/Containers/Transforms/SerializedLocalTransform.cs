using LabFusion.Extensions;
using LabFusion.Network;
using UnityEngine;
using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;

namespace LabFusion.Data
{
    public struct SerializedLocalTransform : IFusionSerializable
    {
        public const int Size = sizeof(float) * 3 + SerializedSmallQuaternion.Size;
        public static readonly SerializedLocalTransform Default = new(Vector3Extensions.zero, Quaternion.identity);

        public SystemVector3 position;
        public SystemQuaternion rotation;

        private SerializedSmallQuaternion _compressedRotation;

        private bool _isValid;

        public bool IsValid => _isValid;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(position);
            writer.Write(_compressedRotation);
        }

        public void Deserialize(FusionReader reader)
        {
            position = reader.ReadSystemVector3();

            _compressedRotation = reader.ReadFromFactory(SerializedSmallQuaternion.Create);
            rotation = _compressedRotation.Expand();

            _isValid = true;
        }

        public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
            : this(localPosition.ToSystemVector3(), localRotation.ToSystemQuaternion()) { }

        public SerializedLocalTransform(Transform transform)
            : this(transform.localPosition.ToSystemVector3(), transform.localRotation.ToSystemQuaternion()) { }

        public SerializedLocalTransform(SystemVector3 localPosition, SystemQuaternion localRotation)
        {
            position = localPosition;
            rotation = localRotation;

            _compressedRotation = SerializedSmallQuaternion.Compress(rotation);

            _isValid = true;
        }
    }
}
