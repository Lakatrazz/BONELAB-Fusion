#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.RPC;
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

        private void Awake()
        {
            gameObject.layer = (int)MarrowLayers.EntityTrigger;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var tracker = Tracker.Cache.Get(other.gameObject);

            if (tracker == null)
            {
                return;
            }

            var marrowEntity = tracker.Entity;

            if (marrowEntity == null)
            {
                return;
            }

            if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
            {
                return;
            }

            OnNetworkEntityEnter(networkEntity);
        }

        private static void OnNetworkEntityEnter(NetworkEntity networkEntity)
        {
            var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

            if (networkPlayer != null)
            {
                OnNetworkPlayerEnter(networkEntity, networkPlayer);
                return;
            }

            var networkProp = networkEntity.GetExtender<NetworkProp>();

            if (networkProp != null)
            {
                OnNetworkPropEnter(networkEntity);
                return;
            }
        }

        private static void OnNetworkPlayerEnter(NetworkEntity networkEntity, NetworkPlayer networkPlayer)
        {
            // Only kill if this is the Local Player
            if (!networkEntity.IsOwner)
            {
                return;
            }

            var health = networkPlayer.RigRefs.Health;

            if (!KillDamageOverride.HasValue || KillDamageOverride.Value == true)
            {
                health.ApplyKillDamage();
            }

            OnKillPlayer?.InvokeSafe("executing OnKillPlayer hook");
        }

        private static void OnNetworkPropEnter(NetworkEntity networkEntity)
        {
            // Only despawn triggered props if this is the host
            if (!NetworkInfo.IsHost)
            {
                return;
            }

            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityID = networkEntity.ID,
                DespawnEffect = true,
            });
        }
#endif
    }
}