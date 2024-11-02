using System.Collections;
using System.Collections.Generic;

#if MELONLOADER
using Il2CppInterop.Runtime.InteropTypes.Fields;

using Il2CppUltEvents;

using LabFusion.Data;

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

        private void Awake()
        {
            _requiresOwnershipCached = requiresOwnership.Get();

            var hash = GameObjectHasher.GetHierarchyHash(gameObject);

            HashTable.AddComponent(hash, this);
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);
        }

        public void InvokeHolder()
        {
            OnVariableChangedHolder?.Invoke();
        }
#else
        public bool requiresOwnership = false;

        public UltEventHolder onVariableChangedHolder;
#endif
    }
}