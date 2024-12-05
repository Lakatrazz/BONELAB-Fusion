using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Entities;

public class RigPose : IFusionSerializable
{
    public SerializedLocalTransform[] trackedPoints = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

    public SerializedSmallQuaternion trackedPlayspace = SerializedSmallQuaternion.Default;

    public BodyPose pelvisPose = new();

    public SerializedController leftController = null;
    public SerializedController rightController = null;

    public float feetOffset = 0f;

    public void ReadSkeleton(RigSkeleton skeleton)
    {
        // Read tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            trackedPoints[i] = new SerializedLocalTransform(skeleton.trackedPoints[i]);
        }

        // Read playspace
        trackedPlayspace = SerializedSmallQuaternion.Compress(skeleton.trackedPlayspace.rotation);

        // Read pelvis
        pelvisPose.position = skeleton.physicsPelvis.position;
        pelvisPose.rotation = skeleton.physicsPelvis.rotation;
        pelvisPose.velocity = skeleton.physicsPelvis.velocity;
        pelvisPose.angularVelocity = skeleton.physicsPelvis.angularVelocity;

        // Read hands
        leftController = new(skeleton.physicsLeftHand.Controller);
        rightController = new(skeleton.physicsRightHand.Controller);

        // Read feet offset
        feetOffset = skeleton.remapRig._feetOffset;
    }

    public void Serialize(FusionWriter writer)
    {
        // Write tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            writer.Write(trackedPoints[i]);
        }

        // Write playspace
        writer.Write(trackedPlayspace);

        // Write pelvis
        writer.Write(pelvisPose);

        // Write hands
        writer.Write(leftController);
        writer.Write(rightController);

        // Write feet offset
        writer.Write(feetOffset);
    }

    public void Deserialize(FusionReader reader)
    {
        // Read tracked points
        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            trackedPoints[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();
        }

        // Read playspace
        trackedPlayspace = reader.ReadFusionSerializable<SerializedSmallQuaternion>();

        // Read pelvis
        pelvisPose = reader.ReadFusionSerializable<BodyPose>();

        // Read hands
        leftController = reader.ReadFusionSerializable<SerializedController>();
        rightController = reader.ReadFusionSerializable<SerializedController>();

        // Read feet offset
        feetOffset = reader.ReadSingle();
    }
}