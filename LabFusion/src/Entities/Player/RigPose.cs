using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Entities;

public class RigPose : IFusionSerializable
{
    public SerializedLocalTransform[] trackedPoints = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

    public SerializedSmallQuaternion trackedPlayspace = SerializedSmallQuaternion.Default;

    public BodyPose pelvisPose = new();

    public SerializedHand physicsLeftHand = null;
    public SerializedHand physicsRightHand = null;

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
        physicsLeftHand = new(skeleton.physicsLeftHand, skeleton.physicsLeftHand.Controller);
        physicsRightHand = new(skeleton.physicsRightHand, skeleton.physicsRightHand.Controller);

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
        writer.Write(physicsLeftHand);
        writer.Write(physicsRightHand);

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
        physicsLeftHand = reader.ReadFusionSerializable<SerializedHand>();
        physicsRightHand = reader.ReadFusionSerializable<SerializedHand>();

        // Read feet offset
        feetOffset = reader.ReadSingle();
    }
}