using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;

using SLZ.Marrow.Pool;

namespace LabFusion.Utilities {
    internal class PooleePusher {
        private readonly List<AssetPoolee> _list = new List<AssetPoolee>();

        public void Push(AssetPoolee poolee) {
            if (!_list.Has(poolee))
                _list.Add(poolee);
        }

        public bool Pull(AssetPoolee poolee) {
            for (var i = 0; i < _list.Count; i++) {
                var found = _list[i];

                if (found == poolee) {
                    _list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool Contains(AssetPoolee poolee)
        {
            for (var i = 0; i < _list.Count; i++)  {
                var found = _list[i];

                if (found == poolee) {
                    return true;
                }
            }

            return false;
        }
    }
}
