using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class ByteExtensions {
        public static sbyte ToSByte(this float single) {
            return (sbyte)(single * 127f);
        }

        public static float ToSingle(this sbyte signedByte) {
            return (float)(signedByte) / 127f;
        }

        public static byte ToByte(this sbyte signedByte) {
            int converted = signedByte;
            converted += 128;
            return (byte)converted;
        }

        public static sbyte ToSByte(this byte unsignedByte) {
            int converted = unsignedByte;
            converted -= 128;
            return (sbyte)converted;
        }
    }
}
