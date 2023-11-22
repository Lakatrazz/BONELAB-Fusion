using System;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class PropCollisionSyncer : MonoBehaviour
    {
        public PropCollisionSyncer(IntPtr intPtr) : base(intPtr) { }

        public PropSyncable syncable;

        private void OnCollisionEnter(Collision collision)
        {
            // Make sure the syncable exists, is held, and is owned by us
            if (!(syncable != null && syncable.IsHeld && syncable.IsOwner()))
                return;

            if (NetworkInfo.HasServer)
            {
                var rb = collision.rigidbody;
                if (!rb)
                    return;

                ImpactUtilities.OnHitRigidbody(rb);
            }
        }
    }
}
