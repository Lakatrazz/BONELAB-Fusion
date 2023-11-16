using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;

namespace LabFusion.Data
{
    public class SerializedTransform : IFusionSerializable
    {
        public const ushort Size = sizeof(float) * 3 + SerializedQuaternion.Size;
        public static readonly SerializedTransform Default = new(Vector3Extensions.zero, Quaternion.identity);

        public SystemVector3 position;
        public SystemQuaternion rotation;

        private SerializedQuaternion _compressedRotation;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(position);
            writer.Write(_compressedRotation);
        }

        public void Deserialize(FusionReader reader)
        {
            position = reader.ReadSystemVector3();

            _compressedRotation = reader.ReadFusionSerializable<SerializedQuaternion>();
            rotation = _compressedRotation.Expand();
        }

        public SerializedTransform() { }

        public SerializedTransform(Vector3 position, Quaternion rotation)
            : this(position.ToSystemVector3(), rotation.ToSystemQuaternion()) { }

        public SerializedTransform(Transform transform)
            : this(transform.position.ToSystemVector3(), transform.rotation.ToSystemQuaternion()) { }

        public SerializedTransform(SystemVector3 position, SystemQuaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;

            this._compressedRotation = SerializedQuaternion.Compress(this.rotation);
        }
    }
}
