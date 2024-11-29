using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

namespace LabFusion.Marrow.Extensions;

public static class MarrowEntityExtensions
{
    public static void ResetPose(this MarrowEntity entity, bool resetVelocity = true)
    {
        for (var i = 0; i < entity._defaultPoseCache.Length; i++)
        {
            var pose = entity._defaultPoseCache[i];
            var body = entity.Bodies[i];

            body.transform.localPosition = pose.position;
            body.transform.localRotation = pose.rotation;

            if (!resetVelocity)
            {
                continue;
            }

            var rigidbody = body._rigidbody;

            if (rigidbody == null)
            {
                continue;
            }

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
