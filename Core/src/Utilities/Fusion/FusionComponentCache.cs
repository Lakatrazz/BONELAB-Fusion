using LabFusion.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Object = UnityEngine.Object;

namespace LabFusion.Utilities {
    public class FusionComponentCache<TSource, TComponent> where TSource : Object where TComponent : class {
        private readonly Dictionary<TSource, TComponent> _Cache = new Dictionary<TSource, TComponent>(new UnityComparer());
        private readonly HashSet<TSource> _HashTable = new HashSet<TSource>(new UnityComparer());

        public IReadOnlyCollection<TComponent> Components => _Cache.Values;

        public TComponent Get(TSource source) {
            if (_HashTable.Contains(source))
                return _Cache[source];
            return null;
        }

        public bool TryGet(TSource source, out TComponent value) {
            if (!_HashTable.Contains(source)) {
                value = null;
                return false;
            }

            value = _Cache[source];
            return true;
        }

        public bool ContainsSource(TSource source) {
            return _HashTable.Contains(source);
        }

        public void Add(TSource source, TComponent component) {
            _HashTable.Add(source);
            _Cache.Add(source, component);
        }

        public void Remove(TSource source) {
            _HashTable.Remove(source);
            _Cache.Remove(source);
        }
    }
}
