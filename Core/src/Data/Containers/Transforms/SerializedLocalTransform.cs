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
        public SerializedSmallVector3 position;
        public SerializedSmallQuaternion rotation;

        public void Serialize(FusionWriter writer) {
            writer.Write(position);
            writer.Write(rotation);
        }

        public void Deserialize(FusionReader reader) {
            position = reader.ReadFusionSerializable<SerializedSmallVector3>();
            rotation = reader.ReadFusionSerializable<SerializedSmallQuaternion>();
        }

        public SerializedLocalTransform() { }

        public SerializedLocalTransform(Vector3 localPosition, Quaternion localRotation)
        {
            this.position = SerializedSmallVector3.Compress(localPosition);
            this.rotation = SerializedSmallQuaternion.Compress(localRotation);
        }

        public SerializedLocalTransform(Transform transform)
        {
            this.position = SerializedSmallVector3.Compress(transform.localPosition);
            this.rotation = SerializedSmallQuaternion.Compress(transform.localRotation);
        }
    }
}
