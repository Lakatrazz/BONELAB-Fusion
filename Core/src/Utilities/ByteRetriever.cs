using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    // Potentially replace with an actually efficient pooling system
    public static class ByteRetriever {
        public const int DefaultSize = 16;

        public static void PopulateInitial() { }

        public static byte[] Rent(int size = DefaultSize) {
            return new byte[size];
        }

        public static void Return(byte[] array) { }
    }
}
