using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSystem.Buffers;

using UnhollowerBaseLib;

namespace LabFusion.Data
{
    internal static class ArrayPooling {
        private static readonly ArrayPool<byte> BytePool = new ArrayPool<byte>();

        /// <summary>
        /// Rents an array of bytes from the ArrayPool for use in fusion messages.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static Il2CppArrayBase<byte> RentBytes(int length = 0) {
            return BytePool.Rent(length);
        }

        /// <summary>
        /// Returns the previously rented array of bytes.
        /// </summary>
        /// <param name="pool"></param>
        internal static void ReturnBytes(in Il2CppArrayBase<byte> pool) {
            BytePool.Return(pool, true);
        }
    }
}
