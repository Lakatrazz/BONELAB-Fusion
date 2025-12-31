using LabFusion.Data;
using LabFusion.Marrow.Serialization;
using LabFusion.Network.Serialization;
using LabFusion.Representation;
using UnityEngine;

namespace LabFusion.Entities;

public class RigPose : INetSerializable
{
    public const int Size = SerializedLocalTransform.Size * RigAbstractor.TransformSyncCount +
        SerializedSmallQuaternion.Size +
        BodyPose.Size +
        SerializedController.Size * 2 +
        sizeof(float) * 4;

    public SerializedLocalTransform[] TrackedPoints = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

    public SerializedSmallQuaternion TrackedPlayspace = SerializedSmallQuaternion.Default;

    public Quaternion TrackedPlayspaceExpanded = Quaternion.identity;

    public BodyPose PelvisPose = new();

    public SerializedController LeftController = null;
    public SerializedController RightController = null;

    public float CrouchTarget = 0f;

    public float FeetOffset = 0f;

    public float Health = 100f;

    public float MaxHealth = 100f;

    public int? GetSize() => Size;

    public void ReadSkeleton(RigSkeleton skeleton)
    {
        // Read tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            TrackedPoints[i] = new SerializedLocalTransform(skeleton.TrackedPoints[i]);
        }

        // Read playspace
        TrackedPlayspaceExpanded = skeleton.TrackedPlayspace.rotation;

        TrackedPlayspace = SerializedSmallQuaternion.Compress(TrackedPlayspaceExpanded);

        // Read bodies
        PelvisPose.ReadFrom(skeleton.PhysicsPelvis);

        // Read hands
        LeftController = new(skeleton.PhysicsLeftHand.Controller);
        RightController = new(skeleton.PhysicsRightHand.Controller);

        // Read extra info
        CrouchTarget = skeleton.RemapRig._crouchTarget;
        FeetOffset = skeleton.RemapRig._feetOffset;
        Health = skeleton.Health.curr_Health;
        MaxHealth = skeleton.Health.max_Health;
    }

    public void Serialize(INetSerializer serializer)
    {
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            var trackedPoint = TrackedPoints[i];

            serializer.SerializeValue(ref trackedPoint);

            TrackedPoints[i] = trackedPoint;
        }

        serializer.SerializeValue(ref TrackedPlayspace);

        if (serializer.IsReader)
        {
            TrackedPlayspaceExpanded = TrackedPlayspace.Expand();
        }

        serializer.SerializeValue(ref PelvisPose);

        serializer.SerializeValue(ref LeftController);
        serializer.SerializeValue(ref RightController);

        serializer.SerializeValue(ref CrouchTarget);
        serializer.SerializeValue(ref FeetOffset);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref MaxHealth);
    }
}