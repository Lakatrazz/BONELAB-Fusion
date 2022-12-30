using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedSmallVector3 : IFusionSerializable
    {
        public ulong value;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(FusionReader reader)
        {
            value = reader.ReadUInt64();
        }

        public static SerializedSmallVector3 Compress(Vector3 vector)
        {
            return new SerializedSmallVector3()
            {
                value = vector.ToULong(true),
            };
        }

        public Vector3 Expand()
        {
            return value.ToVector3();
        }
    }

}
