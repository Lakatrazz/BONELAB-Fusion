using SLZ.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Syncables
{
    public abstract class PropComponentExtender<T> : IPropExtender where T : Component {
        public PropSyncable PropSyncable { get; set; }

        public T Component;

        public bool ValidateExtender(PropSyncable syncable) {
            Component = syncable.GameObject.GetComponentInChildren<T>(true);

            if (Component) {
                RemoveFromCache(Component);
                AddToCache(Component, syncable);
                PropSyncable = syncable;
                return true;
            }

            return false;
        }

        public void OnCleanup() {
            RemoveFromCache(Component);
        }

        protected abstract void AddToCache(T component, PropSyncable syncable);

        protected abstract void RemoveFromCache(T component);

        public virtual void OnOwnedUpdate() { }

        public virtual void OnReceivedUpdate() { }

        public virtual void OnOwnershipTransfer() { }

        public virtual void OnUpdate() { }

        public virtual void OnAttach(Hand hand, Grip grip) { }

        public virtual void OnDetach(Hand hand, Grip grip) { }

        public virtual void OnHeld() { }
    }
}
