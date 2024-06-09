using LabFusion.Extensions;

using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Utilities
{
    internal class PooleePusher
    {
        private readonly List<Poolee> _list = new();

        public void Push(Poolee poolee)
        {
            if (!_list.Has(poolee))
                _list.Add(poolee);
        }

        public bool Pull(Poolee poolee)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var found = _list[i];

                if (found == poolee)
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool Contains(Poolee poolee)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var found = _list[i];

                if (found == poolee)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
