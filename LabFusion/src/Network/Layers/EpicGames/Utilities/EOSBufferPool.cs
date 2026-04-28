using System.Collections.Concurrent;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Buffer pool for packet data.
/// </summary>
internal class EOSBufferPool
{
    private readonly ConcurrentQueue<byte[]> _pool = new();
    private readonly int _maxPoolSize;
    private readonly int _maxBufferSize;

    internal EOSBufferPool(int maxPoolSize = 100, int maxBufferSize = 2340)
    {
        _maxPoolSize = maxPoolSize;
        _maxBufferSize = maxBufferSize;
    }

    internal byte[] Rent(int minimumSize)
    {
        if (_pool.TryDequeue(out var buffer) && buffer.Length >= minimumSize)
            return buffer;

        // Return undersized buffer to pool
        if (buffer != null && buffer.Length < minimumSize)
            Return(buffer);

        return new byte[minimumSize];
    }

    internal void Return(byte[] buffer)
    {
        if (buffer == null)
            return;

        if (_pool.Count < _maxPoolSize && buffer.Length <= _maxBufferSize)
        {
            _pool.Enqueue(buffer);
        }
    }

    internal void Clear()
    {
        while (_pool.TryDequeue(out _)) { }
    }
}