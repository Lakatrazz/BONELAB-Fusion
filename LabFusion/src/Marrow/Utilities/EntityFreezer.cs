using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

namespace LabFusion.Marrow.Utilities;

public sealed class EntityFreezer
{
    public ConfigurableJoint[] FreezeJoints { get; private set; } = null;

    public bool IsFrozen { get; private set; } = false;

    public void Freeze(MarrowBody[] bodies)
    {
        if (IsFrozen)
        {
            return;
        }

        IsFrozen = true;

        FreezeJoints = new ConfigurableJoint[bodies.Length];

        for (var i = 0; i < FreezeJoints.Length; i++)
        {
            var body = bodies[i];

            if (!body.HasRigidbody)
            {
                continue;
            }

            var joint = body.gameObject.AddComponent<ConfigurableJoint>();
            joint.xMotion = joint.yMotion = joint.zMotion
                = joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0f;
            joint.projectionAngle = 0f;

            FreezeJoints[i] = joint;
        }
    }

    public void Unfreeze()
    {
        if (!IsFrozen)
        {
            return;
        }

        IsFrozen = false;

        if (FreezeJoints == null)
        {
            return;
        }

        for (var i = 0; i < FreezeJoints.Length; i++)
        {
            var joint = FreezeJoints[i];

            if (joint == null)
            {
                continue;
            }

            GameObject.Destroy(joint);
        }

        FreezeJoints = null;
    }
}
