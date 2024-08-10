using LabFusion.Data;

namespace LabFusion.Utilities
{
    public static class ByteRetriever
    {
        public const int DefaultSize = 16;

        private static FusionArrayPool<byte> _arrayPool = null;

        public static void PopulateInitial()
        {
            _arrayPool = new FusionArrayPool<byte>();
        }

        public static byte[] Rent(int size = DefaultSize)
        {
            return _arrayPool.Rent(size);
        }

        public static void Return(byte[] array)
        {
            _arrayPool.Return(array);
        }
    }
}
