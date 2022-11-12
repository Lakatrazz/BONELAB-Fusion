using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.Data
{
    public class SerializedLocalTransform : IFusionSerializable
    {
        public const ushort size = (sizeof(ulong) + SerializedQuaternion.size);

        public ulong position;
        public SerializedQuaternion rotation;

        public void Serialize(FusionWriter writer) {
            writer.Write(position);
            writer.Write(rotation);
        }

        public void Deserialize(FusionReader reader) {
            position = reader.ReadUInt64();
            rotation = reader.ReadFusionSerializable<SerializedQuaternion>();
        }

        public SerializedLocalTransform() { }

        public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
        {
            this.position = localPosition.ToULong(true);
            this.rotation = SerializedQuaternion.Compress(localRotation);
        }

        public SerializedLocalTransform(Transform transform)
        {
            this.position = transform.localPosition.ToULong(true);
            this.rotation = SerializedQuaternion.Compress(transform.localRotation);
        }
    }
}
