using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

using SystemVector3 = System.Numerics.Vector3;

namespace LabFusion.Data
{
    public class SerializedTransform : IFusionSerializable
    {
        public const ushort Size = sizeof(float) * 3 + SerializedQuaternion.Size;
        public static readonly SerializedTransform Default = new(Vector3Extensions.zero, Quaternion.identity);

        public SystemVector3 position;
        public SerializedQuaternion rotation;

        public void Serialize(FusionWriter writer) {
            writer.Write(position);
            writer.Write(rotation);
        }

        public void Deserialize(FusionReader reader) {
            position = reader.ReadSystemVector3();
            rotation = reader.ReadFusionSerializable<SerializedQuaternion>();
        }

        public SerializedTransform() { }

        public SerializedTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position.ToSystemVector3();
            this.rotation = SerializedQuaternion.Compress(rotation.ToSystemQuaternion());
        }

        public SerializedTransform(Transform transform)
        {
            this.position = transform.position.ToSystemVector3();
            this.rotation = SerializedQuaternion.Compress(transform.rotation.ToSystemQuaternion());
        }
    }
}
