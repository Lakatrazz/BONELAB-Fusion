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

        public Il2CppReferenceField<UltEvent> onOwnershipGained;

        public Il2CppReferenceField<UltEvent> onOwnershipLost;

        private UltEvent _onOwnershipGainedCached = null;
        private UltEvent _onOwnershipLostCached = null;

        private bool _isOwnerCached = false;

        private void Awake()
        {
            _onOwnershipGainedCached = onOwnershipGained.Get();
            _onOwnershipLostCached = onOwnershipLost.Get();
        }

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
                _onOwnershipGainedCached?.Invoke();
            }
            else
            {
                _onOwnershipLostCached?.Invoke();
            }
        }

        public bool IsOwner()
        {
            return _isOwnerCached;
        }
#else
        public UltEvent onOwnershipGained;

        public UltEvent onOwnershipLost;

        public bool IsOwner()
        {
            return false;
        }
#endif
    }
}