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
        public const float DespawnTime = 60f;

        public TimedDespawner(IntPtr intPtr) : base(intPtr) { }

        public AssetPoolee poolee;

        private float _timeOfRefresh;
        private bool _hasPoolee;

        private void Awake() {
            poolee = GetComponentInParent<AssetPoolee>();
            _hasPoolee = poolee != null;
            Refresh();
        }

        public void Refresh() {
            _timeOfRefresh = Time.realtimeSinceStartup;
        }

        public void LateUpdate() {
            if (NetworkInfo.IsServer && _hasPoolee && Time.realtimeSinceStartup - _timeOfRefresh >= DespawnTime) {
                poolee.Despawn();
            }
        }
    }
}
