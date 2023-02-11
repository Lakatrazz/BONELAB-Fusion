using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;

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
                if (!rb)
                    return;

                ImpactUtilities.OnHitRigidbody(rb);
            }
        }
    }
}
