using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Rig;

namespace LabFusion.Entities;

public class RigPhysics
{
    private MarrowEntity _entity = null;

    public RigPhysics(RigManager rigManager)
    {
        _entity = rigManager.marrowEntity;
    }

    public void CullPhysics(bool isInactive)
    {
        bool isEnabled = !isInactive;
        _entity.EnableTrackers(isEnabled);
        _entity.EnableColliders(isEnabled);
    }
}
