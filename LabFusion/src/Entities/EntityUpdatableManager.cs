using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Entities;

public sealed class EntityUpdatableManager
{
    public RegistrableList<IEntityUpdatable> UpdateManager { get; } = new();
    public RegistrableList<IEntityFixedUpdatable> FixedUpdateManager { get; } = new();
    public RegistrableList<IEntityLateUpdatable> LateUpdateManager { get; } = new();

    public void OnEntityUpdate(float deltaTime)
    {
        foreach (var entity in UpdateManager.Entries)
        {
            try
            {
                entity.OnEntityUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IEntityUpdatable Update", e);
            }
        }
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        foreach (var entity in FixedUpdateManager.Entries)
        {
            try
            {
                entity.OnEntityFixedUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IEntityFixedUpdatable FixedUpdate", e);
            }
        }
    }

    public void OnEntityLateUpdate(float deltaTime)
    {
        foreach (var entity in LateUpdateManager.Entries)
        {
            try
            {
                entity.OnEntityLateUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IEntityLateUpdatable LateUpdate", e);
            }
        }
    }
}
