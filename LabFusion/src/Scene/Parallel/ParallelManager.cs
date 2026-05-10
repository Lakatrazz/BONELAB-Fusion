namespace LabFusion.Scene;

public static class ParallelManager
{
    private static readonly ParallelUpdatableManager _updatableManager = new();
    public static ParallelUpdatableManager UpdatableManager => _updatableManager;

    internal static void OnFixedUpdate(float deltaTime) => UpdatableManager.OnFixedUpdate(deltaTime);
}
