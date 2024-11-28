#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Network;
using LabFusion.Entities;
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

            if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
            {
                return;
            }

            if (!networkEntity.IsOwner)
            {
                return;
            }

            var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

            if (networkEntity == null)
            {
                return;
            }

            var health = networkPlayer.RigRefs.RigManager.health.TryCast<Player_Health>();

            health.ApplyKillDamage();
        }
#endif
    }
}