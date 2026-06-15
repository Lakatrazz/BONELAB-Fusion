namespace LabFusion.Entities;

public interface IEntityPosableExtender : IEntityExtender
{
    void OnPoseReceived(EntityPose pose);
}
