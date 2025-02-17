using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Entities;

public class RigPose : IFusionSerializable
{
    public const int Size = SerializedLocalTransform.Size * RigAbstractor.TransformSyncCount +
        SerializedSmallQuaternion.Size +
        BodyPose.Size * 2 +
        SerializedController.Size * 2 +
        sizeof(float) * 2;

    public SerializedLocalTransform[] TrackedPoints { get; set; } = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

    public SerializedSmallQuaternion TrackedPlayspace { get; set; } = SerializedSmallQuaternion.Default;

    public BodyPose PelvisPose { get; set; } = new();

    public SerializedController LeftController { get; set; } = null;
    public SerializedController RightController { get; set; } = null;

    public float FeetOffset { get; set; } = 0f;

    public float Health { get; set; } = 100f;

    public float MaxHealth { get; set; } = 100f;

    public int? GetSize()
    {
        return Size;
    }

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
        FeetOffset = skeleton.remapRig._feetOffset;
        Health = skeleton.health.curr_Health;
        MaxHealth = skeleton.health.max_Health;
    }

    public void Serialize(FusionWriter writer)
    {
        // Write tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            writer.Write(TrackedPoints[i]);
        }

        // Write playspace
        writer.Write(TrackedPlayspace);

        // Write bodies
        writer.Write(PelvisPose);

        // Write hands
        writer.Write(LeftController);
        writer.Write(RightController);

        // Write extra info
        writer.Write(FeetOffset);
        writer.Write(Health);
        writer.Write(MaxHealth);
    }

    public void Deserialize(FusionReader reader)
    {
        // Read tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            TrackedPoints[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();
        }

        // Read playspace
        TrackedPlayspace = reader.ReadFusionSerializable<SerializedSmallQuaternion>();

        // Read bodies
        PelvisPose = reader.ReadFusionSerializable<BodyPose>();

        // Read hands
        LeftController = reader.ReadFusionSerializable<SerializedController>();
        RightController = reader.ReadFusionSerializable<SerializedController>();

        // Read extra info
        FeetOffset = reader.ReadSingle();
        Health = reader.ReadSingle();
        MaxHealth = reader.ReadSingle();
    }
}