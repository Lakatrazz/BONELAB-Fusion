using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;

using MelonLoader;

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class CollisionSyncer : MonoBehaviour {
        public CollisionSyncer(IntPtr intPtr) : base(intPtr) { }

        private void OnCollisionEnter(Collision collision) {
            if (NetworkInfo.HasServer) {
                var rb = collision.rigidbody;
                if (!rb || PropSyncable.HostCache.ContainsSource(rb.gameObject))
                    return;

                MelonCoroutines.Start(CoWaitAndSync(rb));
            }
        }

        private IEnumerator CoWaitAndSync(Rigidbody rb) {
            for (var i = 0; i < 4; i++)
                yield return null;

            PropSender.SendPropCreation(rb.gameObject);
        }
    }
}
