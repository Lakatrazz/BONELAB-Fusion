#if MELONLOADER
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

using Il2CppUltEvents;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

using MelonLoader;
#else
using UltEvents;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
    // MelonLoader doesn't like injecting abstract classes sometimes
    // So, have it be a regular class in script but abstract in editor
    // That way, the base class cannot be added
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
    public class RPCVariable : MonoBehaviour
#else
    public abstract class RPCVariable : MonoBehaviour
#endif
    {
#if MELONLOADER
        public RPCVariable(IntPtr intPtr) : base(intPtr) { }

        public static readonly ComponentHashTable<RPCVariable> HashTable = new();

        public Il2CppValueField<bool> requiresOwnership;

        public Il2CppReferenceField<UltEventHolder> onVariableChangedHolder;

        private bool _requiresOwnershipCached;

        public bool RequiresOwnership => _requiresOwnershipCached;

        public UltEventHolder OnVariableChangedHolder => onVariableChangedHolder.Get();

        [HideFromIl2Cpp]
        public bool HasNetworkEntity { get; set; } = false;

        private void Awake()
        {
            _requiresOwnershipCached = requiresOwnership.Get();

            var hash = GameObjectHasher.GetHierarchyHash(gameObject);

            HashTable.AddComponent(hash, this);

            CatchupManager.OnPlayerServerCatchup += OnPlayerServerCatchup;
        }

        [HideFromIl2Cpp]
        private void OnPlayerServerCatchup(PlayerID playerID)
        {
            if (HasNetworkEntity)
            {
                return;
            }

            CatchupPlayer(playerID);
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);

            CatchupManager.OnPlayerServerCatchup -= OnPlayerServerCatchup;
        }

        public void InvokeHolder()
        {
            try
            {
                OnVariableChangedHolder?.Invoke();
            }
            catch { }
        }

        [HideFromIl2Cpp]
        public virtual void CatchupPlayer(PlayerID playerID) { }
#else
        public bool requiresOwnership = false;

        public UltEventHolder onVariableChangedHolder;
#endif
    }
}