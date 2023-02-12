using LabFusion.Extensions;
using LabFusion.MonoBehaviours;

using SLZ.Interaction;
using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropCollisionSyncerExtender : IPropExtender {
        public PropSyncable PropSyncable { get; set; }

        public PropCollisionSyncer Component;

        public bool ValidateExtender(PropSyncable syncable) {
            if (syncable.Rigidbodies.Length > 0) {
                PropSyncable = syncable;
                Component = PropSyncable.Rigidbodies[0].gameObject.AddComponent<PropCollisionSyncer>();
                Component.syncable = PropSyncable;
                return true;
            }

            return false;
        }

        public void OnCleanup() {
            if (!Component.IsNOC()) {
                GameObject.Destroy(Component);
            }
        }

        public virtual void OnOwnedUpdate() { }

        public virtual void OnReceivedUpdate() { }

        public virtual void OnOwnershipTransfer() { }

        public virtual void OnUpdate() { }

        public virtual void OnAttach(Hand hand, Grip grip) { }

        public virtual void OnDetach(Hand hand, Grip grip) { }

        public virtual void OnHeld() { }
    }
}
