using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Grabbables {
    public static class InteractableHelpers {
        public static GameObject GetRoot(this InteractableHost host) {
            var poolee = host.GetComponentInParent<AssetPoolee>();
            var ignoreHierarchy = host.GetComponentInParent<IgnoreHierarchy>();

            if (poolee)
                return poolee.gameObject;
            else if (host.manager)
                return host.manager.gameObject;
            else if (ignoreHierarchy)
                return ignoreHierarchy.gameObject;
            else
            {
                var rigidbodies = host.GetComponentsInParent<Rigidbody>(true);
                if (rigidbodies.Length > 0)
                    return rigidbodies.Last().gameObject;
            }

            return host.gameObject;
        }
    }
}
