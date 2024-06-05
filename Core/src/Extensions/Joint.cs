using UnityEngine;

namespace LabFusion.Extensions
{
    public static class JointExtensions
    {
        public static Vector3 GetLocalAnchor(this Joint joint, Vector3 anchor)
        {
            return joint.transform.InverseTransformPoint(anchor);
        }

        public static Vector3 GetLocalConnectedAnchor(this Joint joint, Vector3 anchor)
        {
            return joint.connectedBody ? joint.connectedBody.transform.InverseTransformPoint(anchor) : anchor;
        }

        public static Vector3 GetWorldConnectedAnchor(this Joint joint, Vector3 anchor)
        {
            return joint.connectedBody ? joint.connectedBody.transform.TransformPoint(anchor) : anchor;
        }
    }
}
