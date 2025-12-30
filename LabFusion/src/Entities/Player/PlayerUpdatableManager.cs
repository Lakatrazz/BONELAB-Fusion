using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Entities;

public sealed class PlayerUpdatableManager
{
    public RegistrableList<IPlayerUpdatable> UpdateManager { get; } = new();
    public RegistrableList<IPlayerFixedUpdatable> FixedUpdateManager { get; } = new();
    public RegistrableList<IPlayerLateUpdatable> LateUpdateManager { get; } = new();

    public void OnPlayerUpdate(float deltaTime)
    {
        foreach (var entity in UpdateManager.Entries)
        {
            try
            {
                entity.OnPlayerUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IPlayerUpdatable Update", e);
            }
        }
    }

    public void OnPlayerFixedUpdate(float deltaTime)
    {
        foreach (var entity in FixedUpdateManager.Entries)
        {
            try
            {
                entity.OnPlayerFixedUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IPlayerFixedUpdatable FixedUpdate", e);
            }
        }
    }

    public void OnPlayerLateUpdate(float deltaTime)
    {
        foreach (var entity in LateUpdateManager.Entries)
        {
            try
            {
                entity.OnPlayerLateUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IPlayerLateUpdatable LateUpdate", e);
            }
        }
    }
}
