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
    public class PropCollisionSyncer : MonoBehaviour
    {
        public PropCollisionSyncer(IntPtr intPtr) : base(intPtr) { }

        public PropSyncable syncable;

        private void OnCollisionEnter(Collision collision)
        {
            // The syncable extender will automatically disable this component when it's not held, so no need to check
            if (NetworkInfo.HasServer)
            {
                var rb = collision.rigidbody;
                if (!rb)
                    return;

                ImpactUtilities.OnHitRigidbody(rb);
            }
        }

        private void OnDestroy()
        {
            syncable = null;
        }
    }
}
