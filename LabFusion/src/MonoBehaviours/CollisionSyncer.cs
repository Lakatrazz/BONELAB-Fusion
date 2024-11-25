using LabFusion.Network;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class CollisionSyncer : MonoBehaviour
    {
        public CollisionSyncer(IntPtr intPtr) : base(intPtr) { }

        private void OnCollisionEnter(Collision collision)
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }

            var rb = collision.rigidbody;

            if (!rb)
            {
                return;
            }

            ImpactUtilities.OnHitRigidbody(rb);
        }
    }
}
