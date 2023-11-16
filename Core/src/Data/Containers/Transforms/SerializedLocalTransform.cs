using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Extensions;
using LabFusion.Network;

using SystemVector3 = System.Numerics.Vector3;

namespace LabFusion.Data
{
    public class SerializedLocalTransform : IFusionSerializable
    {
        public const int Size = sizeof(float) * 3 + SerializedSmallQuaternion.Size;
        public static readonly SerializedLocalTransform Default = new(Vector3Extensions.zero, Quaternion.identity);

        public SystemVector3 position;
        public SerializedSmallQuaternion rotation;

        public void Serialize(FusionWriter writer) {
            writer.Write(position);
            writer.Write(rotation);
        }

        public void Deserialize(FusionReader reader) {
            position = reader.ReadSystemVector3();
            rotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
        }

        public SerializedLocalTransform() { }

        public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
        {
            this.position = localPosition.ToSystemVector3();
            this.rotation = SerializedSmallQuaternion.Compress(localRotation.ToSystemQuaternion());
        }

        public SerializedLocalTransform(Transform transform)
        {
            this.position = transform.localPosition.ToSystemVector3();
            this.rotation = SerializedSmallQuaternion.Compress(transform.localRotation.ToSystemQuaternion());
        }
    }
}
