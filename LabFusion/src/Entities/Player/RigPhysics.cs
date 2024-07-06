using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Rig;

namespace LabFusion.Entities;

public class RigPhysics
{
    private readonly MarrowEntity _entity = null;

    public RigPhysics(RigManager rigManager)
    {
        _entity = rigManager.marrowEntity;
    }

    public void CullPhysics(bool isInactive)
    {
        bool isEnabled = !isInactive;
        _entity.EnableColliders(isEnabled);
    }
}
