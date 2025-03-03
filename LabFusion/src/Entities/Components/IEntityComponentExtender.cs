using UnityEngine;

namespace LabFusion.Entities;

public interface IEntityComponentExtender : IEntityExtender
{
    bool TryRegister(NetworkEntity entity, GameObject parent);

    void Unregister();

    void RegisterDynamics(NetworkEntity entity, GameObject parent);

    void UnregisterDynamics();
}
