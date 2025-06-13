#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Data;
using LabFusion.SDK.Extenders;
using LabFusion.Network;
using LabFusion.Scene;
using LabFusion.SDK.Messages;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [RequireComponent(typeof(Animator))]
#endif
    public class AnimatorSyncer : MonoBehaviour
    {
#if MELONLOADER
        public AnimatorSyncer(IntPtr intPtr) : base(intPtr) { }

        public static readonly ComponentHashTable<AnimatorSyncer> HashTable = new();

        [HideFromIl2Cpp]
        public Animator Animator { get; set; } = null;

        [HideFromIl2Cpp]
        public bool HasEntity { get; set; } = false;

        [HideFromIl2Cpp]
        public bool IsOwner { get; set; } = false;

        private float _syncTime = 0f;

        private void Awake()
        {
            Animator = GetComponent<Animator>();

            var hash = GameObjectHasher.GetHierarchyHash(gameObject);

            HashTable.AddComponent(hash, this);
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);
        }

        private void Update()
        {
            if (!NetworkSceneManager.IsLevelNetworked)
            {
                return;
            }

            bool ownership;

            if (HasEntity)
            {
                ownership = IsOwner;
            }
            else
            {
                ownership = NetworkSceneManager.IsLevelHost;
            }

            if (!ownership)
            {
                return;
            }

            if (_syncTime > 0f)
            {
                _syncTime -= Time.deltaTime;
                return;
            }

            SyncAnimator();
        }

        [HideFromIl2Cpp]
        public void ApplyAnimationState(AnimationStateData data)
        {
            Animator.Play(data.StateNameHash, data.Layer, data.NormalizedTime);
        }

        private void SyncAnimator(int layer = 0)
        {
            _syncTime = 0.1f;

            var currentState = Animator.GetCurrentAnimatorStateInfo(layer);

            var nameHash = currentState.shortNameHash;
            var normalizedTime = currentState.normalizedTime;

            var data = new AnimationStateData()
            {
                ComponentData = ComponentPathData.CreateFromComponent<AnimatorSyncer, AnimatorSyncerExtender>(this, HashTable, AnimatorSyncerExtender.Cache),
                StateNameHash = nameHash,
                Layer = layer,
                NormalizedTime = normalizedTime,
            };

            MessageRelay.RelayModule<AnimationStateMessage, AnimationStateData>(data, CommonMessageRoutes.UnreliableToOtherClients);
        }
#endif
    }
}