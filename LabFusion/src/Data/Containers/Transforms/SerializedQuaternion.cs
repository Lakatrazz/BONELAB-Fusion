using UnityEngine;

using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    public class SerializedQuaternion : IFusionSerializable
    {
        public short c1, c2, c3;
        public byte loss; // Lost component in compression

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

        public void Deserialize(FusionReader reader)
        {
            c1 = reader.ReadInt16();
            c2 = reader.ReadInt16();
            c3 = reader.ReadInt16();
            loss = reader.ReadByte();
        }

        public static SerializedQuaternion Compress(Quaternion quat)
        {
            // Based on https://gafferongames.com/post/snapshot_compression/
            // Basically compression works by dropping a component that is the lowest absolute value
            // We first add each component to an array, then sort said array from largest to smallest absolute value

            unsafe
            {
                float* components = stackalloc float[4] { quat.x, quat.y, quat.z, quat.w };

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

                SerializedQuaternion serialized = new()
                {
                    c1 = compressed[0],
                    c2 = compressed[1],
                    c3 = compressed[2],
                    loss = dropped
                };

                return serialized;
            }
        }

        public Quaternion Expand()
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
                0 => new Quaternion(f4, f1, f2, f3),
                1 => new Quaternion(f1, f4, f2, f3),
                2 => new Quaternion(f1, f2, f4, f3),
                3 => new Quaternion(f1, f2, f3, f4),
                _ => QuaternionExtensions.identity,
            };
        }
    }
}

