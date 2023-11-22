using System.Linq;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Utilities;
using SLZ.VFX;
using UnityEngine;

namespace LabFusion.Grabbables
{
    public static class InteractableHelpers
    {
        public static GameObject GetSyncRoot(this GameObject go)
        {
            var poolee = go.GetComponentInParent<AssetPoolee>();
            var blip = go.GetComponentInParent<Blip>();
            var ignoreHierarchy = go.GetComponentInParent<IgnoreHierarchy>();

            var host = go.GetComponentInParent<InteractableHost>();
            if (!host)
                host = go.GetComponentInChildren<InteractableHost>();

            go = host ? host.gameObject : go;

            if (poolee)
                return poolee.gameObject;
            if (blip)
                return blip.gameObject;
            if (host && host.manager)
                return host.manager.gameObject;
            if (ignoreHierarchy)
                return ignoreHierarchy.gameObject;
            var rigidbodies = go.GetComponentsInParent<Rigidbody>(true);
            if (rigidbodies.Length > 0)
                return rigidbodies.Last().gameObject;

            return go;
        }

        public static GameObject GetSyncRoot(this InteractableHost host)
        {
            return host.gameObject.GetSyncRoot();
        }
    }
}
