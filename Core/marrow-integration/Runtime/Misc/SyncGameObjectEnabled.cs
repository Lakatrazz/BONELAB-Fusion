using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Syncables;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Sync GameObject Enabled")]
    [DisallowMultipleComponent]
#endif
    public sealed class SyncGameObjectEnabled : FusionMarrowBehaviour {
#if MELONLOADER
        public SyncGameObjectEnabled(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, SyncGameObjectEnabled> Cache = new FusionComponentCache<GameObject, SyncGameObjectEnabled>();

        public PropSyncable PropSyncable;

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }

        private void OnEnable() {
            if (NetworkInfo.HasServer) {
                // Check syncable
                if (PropSyncable != null) {
                    if (PropSyncable.IsOwner())
                        SDKSender.SendGameObjectActive(true, this);
                }
                else if (NetworkInfo.IsServer) {
                    SDKSender.SendGameObjectActive(true, this);
                }
            }
        }

        private void OnDisable() {
            if (NetworkInfo.HasServer) {
                // Check syncable
                if (PropSyncable != null) {
                    if (PropSyncable.IsOwner())
                        SDKSender.SendGameObjectActive(false, this);
                }
                else if (NetworkInfo.IsServer)
                {
                    SDKSender.SendGameObjectActive(false, this);
                }
            }
        }
#else
        public override string Comment => "Attempts to sync when this GameObject is enabled and disabled.";
#endif
    }
}
