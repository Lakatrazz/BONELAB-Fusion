using System.Collections.Concurrent;

namespace LabFusion.Data
{
    public class FusionArrayPool<T>
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<T[]>> _pool = new();

        public T[] Rent(int size)
        {
            if (!_pool.TryGetValue(size, out ConcurrentQueue<T[]> queue))
            {
                return new T[size];
            }

            if (queue.TryDequeue(out var result))
            {
                return result;
            }
            else
            {
                return new T[size];
            }
        }

        public void Return(T[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            int size = buffer.Length;
            if (!_pool.TryGetValue(size, out ConcurrentQueue<T[]> queue))
            {
                queue = new ConcurrentQueue<T[]>();
                _pool.TryAdd(size, queue);
            }

            queue.Enqueue(buffer);
        }
    }
}
