using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Entities;
#else
using UltEvents;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class OwnershipEvents : MonoBehaviour
    {
#if MELONLOADER
        public OwnershipEvents(IntPtr intPtr) : base(intPtr) { }

        public Il2CppReferenceField<UltEventHolder> onOwnershipGainedHolder;

        public Il2CppReferenceField<UltEventHolder> onOwnershipLostHolder;

        [HideFromIl2Cpp]
        public NetworkEntity Entity { get; set; } = null;

        private bool _isOwner = false;
        private bool _hasSetOwner = false;

        public void OnOwnerChanged(bool owner)
        {
            // Don't track when there isn't any change
            if (_hasSetOwner && owner == _isOwner)
            {
                return;
            }

            _isOwner = owner;
            _hasSetOwner = true;

            if (owner)
            {
                onOwnershipGainedHolder.Get()?.Invoke();
            }
            else
            {
                onOwnershipLostHolder.Get()?.Invoke();
            }
        }

        public bool IsOwner()
        {
            return _isOwner;
        }

        public void TakeOwnership()
        {
            if (Entity == null)
            {
                return;
            }

            NetworkEntityManager.TakeOwnership(Entity);
        }

        public int GetOwner()
        {
            if (Entity == null || !Entity.HasOwner)
            {
                return -1;
            }

            return Entity.OwnerID.SmallID;
        }
#else
        public UltEventHolder onOwnershipGainedHolder;

        public UltEventHolder onOwnershipLostHolder;

        public bool IsOwner()
        {
            return false;
        }

        public void TakeOwnership()
        {
        }

        public int GetOwner()
        {
            return -1;
        }
#endif
    }
}