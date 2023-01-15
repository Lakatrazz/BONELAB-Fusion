using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Utilities;
using SLZ.Vehicle;
using SLZ.VFX;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Grabbables {
    public static class InteractableHelpers {
        public static GameObject GetSyncRoot(this GameObject go) {
            var poolee = go.GetComponentInParent<AssetPoolee>();
            var blip = go.GetComponentInParent<Blip>();
            var ignoreHierarchy = go.GetComponentInParent<IgnoreHierarchy>();

            var host = go.GetComponentInParent<InteractableHost>();
            if (!host)
                host = go.GetComponentInChildren<InteractableHost>();

            go = host ? host.gameObject : go;

            if (poolee)
                return poolee.gameObject;
            else if (blip)
                return blip.gameObject;
            else if (host && host.manager)
                return host.manager.gameObject;
            else if (ignoreHierarchy)
                return ignoreHierarchy.gameObject;
            else {
                var rigidbodies = go.GetComponentsInParent<Rigidbody>(true);
                if (rigidbodies.Length > 0)
                    return rigidbodies.Last().gameObject;
            }

            return go;
        }

        public static GameObject GetSyncRoot(this InteractableHost host) {
            return host.gameObject.GetSyncRoot();
        }
    }
}
