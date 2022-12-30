using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Syncables
{
    public abstract class PropComponentExtender<T> : IPropExtender where T : Component {
        public T Component;

        public bool ValidateExtender(PropSyncable syncable) {
            Component = syncable.GameObject.GetComponentInChildren<T>(true);

            if (Component) {
                RemoveFromCache(Component);
                AddToCache(Component, syncable);
                return true;
            }

            return false;
        }

        public void OnCleanup() {
            RemoveFromCache(Component);
        }

        protected abstract void AddToCache(T component, PropSyncable syncable);

        protected abstract void RemoveFromCache(T component);
    }
}
