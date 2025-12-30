namespace LabFusion.Entities;

public interface IPlayerUpdatable
{
    void OnPlayerUpdate(float deltaTime);
}

public interface IPlayerFixedUpdatable
{
    void OnPlayerFixedUpdate(float deltaTime);
}

public interface IPlayerLateUpdatable
{
    void OnPlayerLateUpdate(float deltaTime);
}