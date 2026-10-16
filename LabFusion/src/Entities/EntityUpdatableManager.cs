using LabFusion.Data;
using LabFusion.Utilities;

using System.Runtime.InteropServices;

namespace LabFusion.Entities;

public sealed class EntityUpdatableManager
{
    public RegistrableList<IEntityUpdatable> UpdateManager { get; } = new();
    public RegistrableList<IEntityFixedUpdatable> FixedUpdateManager { get; } = new();
    public RegistrableList<IEntityLateUpdatable> LateUpdateManager { get; } = new();

    public void OnEntityUpdate(float deltaTime)
    {
        // Microoptimization, skips certain safety checks
        Span<IEntityUpdatable> span = CollectionsMarshal.AsSpan(UpdateManager.Entries);
        
        foreach (var entity in span)
        {
            OnEntityUpdate(entity, deltaTime);
        }
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        // Microoptimization, skips certain safety checks
        Span<IEntityFixedUpdatable> span = CollectionsMarshal.AsSpan(FixedUpdateManager.Entries);

        foreach (var entity in span)
        {
            OnEntityFixedUpdate(entity, deltaTime);
        }
    }

    public void OnEntityLateUpdate(float deltaTime)
    {
        // Microoptimization, skips certain safety checks
        Span<IEntityLateUpdatable> span = CollectionsMarshal.AsSpan(LateUpdateManager.Entries);

        foreach (var entity in span)
        {
            OnEntityLateUpdate(entity, deltaTime);
        }
    }

    private static void OnEntityUpdate(IEntityUpdatable entity, float deltaTime)
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

    private static void OnEntityFixedUpdate(IEntityFixedUpdatable entity, float deltaTime)
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

    private static void OnEntityLateUpdate(IEntityLateUpdatable entity, float deltaTime)
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
