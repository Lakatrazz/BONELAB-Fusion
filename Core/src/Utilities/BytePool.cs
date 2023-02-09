using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public static class BytePool {
        private const int _warmupSize = 128;
        private const int _poolSize = 1024;

        public const int DefaultSize = 16;

        private static readonly Queue<byte[]> _pool = new Queue<byte[]>(_poolSize);

        public static void PopulateInitial() {
            for (var i = 0; i < _warmupSize; i++) {
                _pool.Enqueue(new byte[DefaultSize]);
            }
        }

        public static byte[] Rent(int size = DefaultSize) {
            if (_pool.Count <= 0)
                return new byte[size];
            else {
                var array = _pool.Dequeue();

                if (array.Length != size) {
                    Array.Resize(ref array, size);
                }

                return array;
            }
        }

        public static void Return(byte[] array) {
            _pool.Enqueue(array);
        }
    }
}
