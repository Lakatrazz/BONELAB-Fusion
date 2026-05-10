using LabFusion.Data;
using LabFusion.Utilities;

namespace LabFusion.Scene;

public sealed class ParallelUpdatableManager
{
    public RegistrableList<IParallelFixedUpdatable> FixedUpdateManager { get; } = new();

    private float _latestDeltaTime = 0f;

    public void OnFixedUpdate(float deltaTime)
    {
        _latestDeltaTime = deltaTime;

        foreach (var entry in FixedUpdateManager.Entries)
        {
            try
            {
                entry.OnPreParallelFixedUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IParallelFixedUpdatable.OnPreParallelFixedUpdate", e);
            }
        }

        Parallel.For(0, FixedUpdateManager.Entries.Count, OnParallelFixedUpdate);

        foreach (var entry in FixedUpdateManager.Entries)
        {
            try
            {
                entry.OnPostParallelFixedUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running IParallelFixedUpdatable.OnPostParallelFixedUpdate", e);
            }
        }
    }

    private void OnParallelFixedUpdate(int index)
    {
        try
        {
            var entry = FixedUpdateManager.Entries[index];
            entry.OnParallelFixedUpdate(_latestDeltaTime);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running IParallelFixedUpdatable.OnParallelFixedUpdate", e);
        }
    }
}
