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
    public readonly struct SerializedQuaternion : IFusionWritable
    {
        public readonly short c1, c2, c3;
        public readonly byte loss; // Lost component in compression

        public const ushort Size = sizeof(short) * 3 + sizeof(byte);

        // The amount we multiply / divide by to preserve precision when using shorts
        public const float PRECISION_OFFSET = 10000.0f;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(c1);
            writer.Write(c2);
            writer.Write(c3);
            writer.Write(loss);
        }

        public static SerializedQuaternion Create(FusionReader reader)
        {
            return new SerializedQuaternion(reader);
        }

        private SerializedQuaternion(FusionReader reader) {
            c1 = reader.ReadInt16();
            c2 = reader.ReadInt16();
            c3 = reader.ReadInt16();
            loss = reader.ReadByte();
        }

        private SerializedQuaternion(short c1, short c2, short c3, byte loss) {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.loss = loss;
        }

        public static SerializedQuaternion Compress(SystemQuaternion quat)
        {
            // Based on https://gafferongames.com/post/snapshot_compression/
            // Basically compression works by dropping a component that is the lowest absolute value
            // We first add each component to an array, then sort said array from largest to smallest absolute value

            unsafe
            {
                float* components = stackalloc float[4] { quat.X, quat.Y, quat.Z, quat.W };

                byte dropped = 0;
                float biggest = 0.0f;
                float sign = 0.0f;
                for (byte c = 0; c < 4; c++)
                {
                    if (Math.Abs(components[c]) > biggest)
                    {
                        sign = (components[c] < 0) ? -1 : 1;

                        dropped = c;
                        biggest = components[c];
                    }
                }

                short* compressed = stackalloc short[3];

                int compIndex = 0;
                for (int c = 0; c < 4; c++)
                {
                    if (c == dropped)
                        continue;

                    compressed[compIndex++] = (short)(components[c] * sign * PRECISION_OFFSET);
                }

                SerializedQuaternion serialized = new(compressed[0], compressed[1], compressed[2], dropped);

                return serialized;
            }
        }

        public SystemQuaternion Expand()
        {
            if (loss >= 4)
                throw new DataCorruptionException($"Expanding a quaternion led to a lost component of {loss}!");

            static float Pow(float x) => x * x;

            float f1 = c1 / PRECISION_OFFSET;
            float f2 = c2 / PRECISION_OFFSET;
            float f3 = c3 / PRECISION_OFFSET;

            float f4 = (float)Math.Sqrt(1f - Pow(f1) - Pow(f2) - Pow(f3));

            // Still dumb...
            return loss switch
            {
                0 => new SystemQuaternion(f4, f1, f2, f3),
                1 => new SystemQuaternion(f1, f4, f2, f3),
                2 => new SystemQuaternion(f1, f2, f4, f3),
                3 => new SystemQuaternion(f1, f2, f3, f4),
                _ => SystemQuaternion.Identity,
            };
        }
    }
}

