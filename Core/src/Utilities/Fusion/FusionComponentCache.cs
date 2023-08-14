using LabFusion.Data;
using LabFusion.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Object = UnityEngine.Object;

namespace LabFusion.Utilities {
    public class FusionComponentCache<TSource, TComponent> where TSource : Object where TComponent : class {
        private readonly FusionDictionary<TSource, TComponent> _Cache = new(new UnityComparer());
        private readonly HashSet<TSource> _HashTable = new(new UnityComparer());

        public ICollection<TComponent> Components => _Cache.Values;

        public TComponent Get(TSource source) {
            if (_HashTable.ContainsIL2CPP(source))
                return _Cache[source];
            return null;
        }

        public bool TryGet(TSource source, out TComponent value) {
            if (!_HashTable.ContainsIL2CPP(source)) {
                value = null;
                return false;
            }

            value = _Cache[source];
            return true;
        }

        public bool ContainsSource(TSource source) {
            return _HashTable.ContainsIL2CPP(source);
        }

        public void Add(TSource source, TComponent component) {
            if (_Cache.ContainsKey(source)) {
                _Cache[source] = component;

#if DEBUG
                FusionLogger.Warn("Attempted to add component to a ComponentCache, but Source already existed. This is probably fine.");
#endif

                return;
            }

            _HashTable.Add(source);
            _Cache.Add(source, component);
        }

        public void Remove(TSource source) {
            _HashTable.RemoveIL2CPP(source);
            _Cache.Remove(source);
        }
    }
}
