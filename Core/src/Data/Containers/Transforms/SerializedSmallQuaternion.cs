using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    public class SerializedSmallQuaternion : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 4;
        public static readonly SerializedSmallQuaternion Default = SerializedSmallQuaternion.Compress(Quaternion.identity);

        public sbyte c1, c2, c3, c4;

        public void Serialize(FusionWriter writer) {
            writer.Write(c1.ToByte());
            writer.Write(c2.ToByte());
            writer.Write(c3.ToByte());
            writer.Write(c4.ToByte());
        }

        public void Deserialize(FusionReader reader) {
            c1 = reader.ReadByte().ToSByte();
            c2 = reader.ReadByte().ToSByte();
            c3 = reader.ReadByte().ToSByte();
            c4 = reader.ReadByte().ToSByte();
        }

        public static SerializedSmallQuaternion Compress(Quaternion quat)
        {
            return new SerializedSmallQuaternion
            {
                c1 = quat.x.ToSByte(),
                c2 = quat.y.ToSByte(),
                c3 = quat.z.ToSByte(),
                c4 = quat.w.ToSByte()
            };
        }

        public Quaternion Expand() {
            return QuaternionExtensions.Normalize(new Quaternion(c1.ToSingle(), c2.ToSingle(), c3.ToSingle(), c4.ToSingle()));
        }
    }
}

