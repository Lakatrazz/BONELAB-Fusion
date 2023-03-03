using SLZ.Interaction;
using SLZ.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Syncables
{
    public abstract class PropComponentArrayExtender<T> : IPropExtender where T : Component {
        public PropSyncable PropSyncable { get; set; }
        
        public T[] Components;

        public bool ValidateExtender(PropSyncable syncable) {
            Components = syncable.GameObject.GetComponentsInChildren<T>(true);

            if (Components.Length > 0) {
                foreach (var comp in Components) {
                    RemoveFromCache(comp);
                    AddToCache(comp, syncable);
                    PropSyncable = syncable;
                }
                return true;
            }

            return false;
        }

        public void OnCleanup() {
            foreach (var comp in Components) {
                if (comp != null)
                    RemoveFromCache(comp);
            }
        }

        public byte? GetIndex(T component) {
            for (byte i = 0; i < Components.Length; i++) {
                if (Components[i] == component)
                    return i;
            }
            return null;
        }

        public T GetComponent(byte index) {
            if (Components != null && Components.Length > index)
                return Components[index];
            return null;
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
