using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Network;
using LabFusion.Extensions;

using SystemQuaternion = System.Numerics.Quaternion;

namespace LabFusion.Data
{
    public readonly struct SerializedSmallQuaternion : IFusionWritable
    {
        public const int Size = sizeof(byte) * 4;
        public static readonly SerializedSmallQuaternion Default = Compress(SystemQuaternion.Identity);

        public readonly sbyte c1, c2, c3, c4;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(c1.ToByte());
            writer.Write(c2.ToByte());
            writer.Write(c3.ToByte());
            writer.Write(c4.ToByte());
        }

        public static SerializedSmallQuaternion Create(FusionReader reader)
        {
            return new SerializedSmallQuaternion(
                reader.ReadByte().ToSByte(),
                reader.ReadByte().ToSByte(),
                reader.ReadByte().ToSByte(),
                reader.ReadByte().ToSByte()
            );
        }

        private SerializedSmallQuaternion(sbyte c1, sbyte c2, sbyte c3, sbyte c4) {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.c4 = c4;
        }

        public static SerializedSmallQuaternion Compress(SystemQuaternion quat)
        {
            return new SerializedSmallQuaternion(
                quat.X.ToSByte(),
                quat.Y.ToSByte(),
                quat.Z.ToSByte(),
                quat.W.ToSByte()
            );
        }

        public SystemQuaternion Expand()
        {
            return SystemQuaternion.Normalize(new SystemQuaternion(c1.ToSingle(), c2.ToSingle(), c3.ToSingle(), c4.ToSingle()));
        }
    }
}

