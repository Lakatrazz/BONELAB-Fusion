#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [RequireComponent(typeof(Collider))]
#endif
    public class DeathTrigger : MonoBehaviour
    {
#if MELONLOADER
        public DeathTrigger(IntPtr intPtr) : base(intPtr) { }

        public static event Action OnKillPlayer;

        public static bool? KillDamageOverride { get; set; } = null;

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var attachedRigidbody = other.attachedRigidbody;

            if (attachedRigidbody == null)
            {
                return;
            }

            var marrowBody = MarrowBody.Cache.Get(attachedRigidbody.gameObject);

            if (marrowBody == null)
            {
                return;
            }

            var marrowEntity = marrowBody.Entity;

            if (marrowEntity == null)
            {
                return;
            }

            if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
            {
                return;
            }

            if (!networkEntity.IsOwner)
            {
                return;
            }

            var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

            if (networkPlayer == null)
            {
                return;
            }

            // Only trigger for head
            if (attachedRigidbody != networkPlayer.RigRefs.RigManager.physicsRig.torso._headRb)
            {
                return;
            }

            var health = networkPlayer.RigRefs.RigManager.health.TryCast<Player_Health>();

            if (!KillDamageOverride.HasValue || KillDamageOverride.Value == true)
            {
                health.ApplyKillDamage();
            }

            OnKillPlayer?.InvokeSafe("executing OnKillPlayer hook");
        }
#endif
    }
}