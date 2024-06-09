using LabFusion.Extensions;
using LabFusion.MonoBehaviours;
using LabFusion.Utilities;
using Il2CppSLZ.Interaction;
using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropCollisionSyncerExtender : IPropExtender
    {
        public PropSyncable PropSyncable { get; set; }

        public CollisionSyncer Component = null;

        private bool _hasComponent = false;

        public bool ValidateExtender(PropSyncable syncable)
        {
            if (syncable.GameObjectCount > 0)
            {
                PropSyncable = syncable;
                return true;
            }

            return false;
        }

        public void ToggleComponent(bool enabled)
        {
            if (enabled == _hasComponent)
            {
                return;
            }

            if (enabled)
            {
                Component = PropSyncable.TempRigidbodies.Items[0].GameObject.AddComponent<CollisionSyncer>();
                _hasComponent = true;
            }
            else
            {
                GameObject.Destroy(Component);
                _hasComponent = false;
            }
        }

        public void OnCleanup()
        {
            if (!Component.IsNOC())
            {
                GameObject.Destroy(Component);
                _hasComponent = false;
            }
        }

        public virtual void OnOwnedUpdate() { }

        public virtual void OnReceivedUpdate() { }

        public virtual void OnOwnershipTransfer() { }

        public virtual void OnUpdate() { }

        public virtual void OnAttach(Hand hand, Grip grip) 
        {
            if (hand.manager.IsSelf())
            {
                ToggleComponent(true);
            }
        }

        public virtual void OnDetach(Hand hand, Grip grip) 
        { 
            if (!PropSyncable.IsGrabbedBy(hand.manager))
            {
                ToggleComponent(false);
            }
        }

        public virtual void OnHeld() { }
    }
}
