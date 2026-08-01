using System.Collections.Concurrent;

namespace LabFusion.Network;

internal sealed class EOSBufferPool
{
    private static readonly int[] BucketSizes = { 64, 256, 512, 768, 1024, 1200, 1500 };
    
    private const int MaxPerBucket = 256;

    private readonly ConcurrentBag<byte[]>[] _buckets;

    internal EOSBufferPool()
    {
        _buckets = new ConcurrentBag<byte[]>[BucketSizes.Length];
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new ConcurrentBag<byte[]>();
        }
    }
    
    internal byte[] Rent(int minimumSize)
    {
        if (minimumSize <= 0)
            return Array.Empty<byte>();

        int bucketIndex = GetBucketIndex(minimumSize);

        if (bucketIndex >= 0 && _buckets[bucketIndex].TryTake(out var buffer))
        {
            return buffer;
        }

        int allocSize = bucketIndex >= 0 ? BucketSizes[bucketIndex] : minimumSize;
        return new byte[allocSize];
    }
    
    internal void Return(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return;

        int bucketIndex = Array.IndexOf(BucketSizes, buffer.Length);
        if (bucketIndex < 0)
            return;

        var bucket = _buckets[bucketIndex];
        if (bucket.Count < MaxPerBucket)
        {
            bucket.Add(buffer);
        }
    }

    private static int GetBucketIndex(int minimumSize)
    {
        for (int i = 0; i < BucketSizes.Length; i++)
        {
            if (BucketSizes[i] >= minimumSize)
                return i;
        }

        return -1;
    }
}