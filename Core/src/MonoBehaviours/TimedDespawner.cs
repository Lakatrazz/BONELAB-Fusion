using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;

using MelonLoader;

using SLZ.Marrow.Pool;

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class TimedDespawner : MonoBehaviour {
        public const float DefaultDespawnTime = 60f;

        public TimedDespawner(IntPtr intPtr) : base(intPtr) { }

        public AssetPoolee poolee;
        public PropSyncable syncable;
        public float totalTime = DefaultDespawnTime;

        private float _timeOfRefresh;
        private bool _hasPoolee;
        private bool _hasSyncable;

        private void Awake() {
            poolee = GetComponentInParent<AssetPoolee>();
            _hasPoolee = poolee != null;
            Refresh();
        }

        public void Refresh() {
            _timeOfRefresh = Time.realtimeSinceStartup;
        }

        public void LateUpdate() {
            // If we had a syncable and it became null, destroy this
            if (_hasSyncable && (syncable == null || syncable.IsDestroyed())) {
                GameObject.Destroy(this);
                return;
            }

            // Store when we get a syncable
            if (!_hasSyncable) {
                _hasSyncable = syncable != null;
            }

            if (NetworkInfo.IsServer && _hasPoolee && Time.realtimeSinceStartup - _timeOfRefresh >= totalTime) {
                poolee.Despawn();
            }
        }
    }
}
