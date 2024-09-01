using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;
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

        private bool _isOwnerCached = false;

        public void OnOwnerChanged(bool owner)
        {
            // Don't track when there isn't any change
            if (owner == _isOwnerCached)
            {
                return;
            }

            _isOwnerCached = owner;

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
            return _isOwnerCached;
        }
#else
        public UltEventHolder onOwnershipGainedHolder;

        public UltEventHolder onOwnershipLostHolder;

        public bool IsOwner()
        {
            return false;
        }
#endif
    }
}