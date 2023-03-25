using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data
{
    public class FusionArrayPool<T>
    {
        private readonly Dictionary<int, Queue<T[]>> _pool = new Dictionary<int, Queue<T[]>>();

        public T[] Rent(int size)
        {
            if (!_pool.TryGetValue(size, out Queue<T[]> queue) || queue.Count == 0)
            {
                return new T[size];
            }

            return queue.Dequeue();
        }

        public void Return(T[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            int size = buffer.Length;
            if (!_pool.TryGetValue(size, out Queue<T[]> queue))
            {
                queue = new Queue<T[]>();
                _pool.Add(size, queue);
            }

            queue.Enqueue(buffer);
        }
    }
}
