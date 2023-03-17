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
        public const int Size = sizeof(float) * 3 + SerializedSmallQuaternion.Size;
        public static readonly SerializedLocalTransform Default = new SerializedLocalTransform(Vector3Extensions.zero, Quaternion.identity);

        public Vector3 position;
        public SerializedSmallQuaternion rotation;

        public void Serialize(FusionWriter writer) {
            writer.Write(position);
            writer.Write(rotation);
        }

        public void Deserialize(FusionReader reader) {
            position = reader.ReadVector3();
            rotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
        }

        public SerializedLocalTransform() { }

        public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
        {
            this.position = localPosition;
            this.rotation = SerializedSmallQuaternion.Compress(localRotation);
        }

        public SerializedLocalTransform(Transform transform)
        {
            this.position = transform.localPosition;
            this.rotation = SerializedSmallQuaternion.Compress(transform.localRotation);
        }
    }
}
