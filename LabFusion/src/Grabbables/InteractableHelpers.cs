using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Grabbables
{
    public static class InteractableHelpers
    {
        public static GameObject GetSyncRoot(this GameObject go)
        {
            var marrowEntity = go.GetComponentInParent<MarrowEntity>(true);
            var poolee = go.GetComponentInParent<Poolee>(true);
            var blip = go.GetComponentInParent<Blip>(true);

            var host = go.GetComponentInParent<InteractableHost>(true);
            if (!host)
                host = go.GetComponentInChildren<InteractableHost>(true);

            go = host ? host.gameObject : go;

            if (marrowEntity)
                return marrowEntity.gameObject;
            else if (poolee)
                return poolee.gameObject;
            else if (blip)
                return blip.gameObject;
            else if (host && host.manager)
                return host.manager.gameObject;
            else
            {
                var rigidbodies = go.GetComponentsInParent<Rigidbody>(true);
                if (rigidbodies.Length > 0)
                    return rigidbodies.Last().gameObject;
            }

            return go;
        }

        public static GameObject GetSyncRoot(this InteractableHost host)
        {
            return host.gameObject.GetSyncRoot();
        }
    }
}
