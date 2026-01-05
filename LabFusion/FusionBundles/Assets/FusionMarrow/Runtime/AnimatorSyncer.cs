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

        public const float MaxDesyncSeconds = 0.05f;

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

            if (!NetworkTickManager.IsTickThisFrame)
            {
                return;
            }

            SyncLayers();
        }

        [HideFromIl2Cpp]
        public void ApplyAnimationState(AnimationStateData data)
        {
            var stateNameHash = data.StateNameHash;
            var layer = data.Layer;
            var normalizedTime = data.NormalizedTime;

            var currentState = Animator.GetCurrentAnimatorStateInfo(layer);

            // If the current state matches the target state, then we can compare the desync of the normalized time
            // If its not too large, then we don't need to update the animator state
            // That way, animators can stay smooth, but sync back up when needed
            if (currentState.shortNameHash == stateNameHash)
            {
                float length = currentState.length;

                float desyncSeconds = MathF.Abs((normalizedTime - currentState.normalizedTime) * length);

                if (desyncSeconds <= MaxDesyncSeconds)
                {
                    return;
                }
            }

            Animator.Play(data.StateNameHash, data.Layer, data.NormalizedTime);
        }

        private void SyncLayers()
        {
            for (var i = 0; i < Animator.layerCount; i++)
            {
                SyncLayer(i);
            }
        }

        private void SyncLayer(int layer)
        {
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