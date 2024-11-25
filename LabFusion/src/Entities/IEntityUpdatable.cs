namespace LabFusion.Entities;

public interface IEntityUpdatable
{
    void OnEntityUpdate(float deltaTime);
}

public interface IEntityFixedUpdatable
{
    void OnEntityFixedUpdate(float deltaTime);
}

public interface IEntityLateUpdatable
{
    void OnEntityLateUpdate(float deltaTime);
}