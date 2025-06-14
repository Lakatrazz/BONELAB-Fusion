using LabFusion.Data;
using LabFusion.Marrow.Serialization;
using LabFusion.Network.Serialization;
using LabFusion.Representation;

namespace LabFusion.Entities;

public class RigPose : INetSerializable
{
    public const int Size = SerializedLocalTransform.Size * RigAbstractor.TransformSyncCount +
        SerializedSmallQuaternion.Size +
        BodyPose.Size * 2 +
        SerializableController.Size * 2 +
        sizeof(float) * 2;

    public SerializedLocalTransform[] TrackedPoints = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

    public SerializedSmallQuaternion TrackedPlayspace = SerializedSmallQuaternion.Default;

    public BodyPose PelvisPose = new();

    public SerializableController LeftController = null;
    public SerializableController RightController = null;

    public float CrouchTarget = 0f;

    public float FeetOffset = 0f;

    public float Health = 100f;

    public float MaxHealth = 100f;

    public void ReadSkeleton(RigSkeleton skeleton)
    {
        // Read tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            TrackedPoints[i] = new SerializedLocalTransform(skeleton.trackedPoints[i]);
        }

        // Read playspace
        TrackedPlayspace = SerializedSmallQuaternion.Compress(skeleton.trackedPlayspace.rotation);

        // Read bodies
        PelvisPose.ReadFrom(skeleton.physicsPelvis);

        // Read hands
        LeftController = new(skeleton.physicsLeftHand.Controller);
        RightController = new(skeleton.physicsRightHand.Controller);

        // Read extra info
        CrouchTarget = skeleton.remapRig._crouchTarget;
        FeetOffset = skeleton.remapRig._feetOffset;
        Health = skeleton.health.curr_Health;
        MaxHealth = skeleton.health.max_Health;
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

        serializer.SerializeValue(ref PelvisPose);

        serializer.SerializeValue(ref LeftController);
        serializer.SerializeValue(ref RightController);

        serializer.SerializeValue(ref CrouchTarget);
        serializer.SerializeValue(ref FeetOffset);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref MaxHealth);
    }
}