using UnityEngine;

namespace LabFusion.Entities;

public interface IEntityComponentExtender : IEntityExtender
{
    bool TryRegister(NetworkEntity networkEntity, GameObject[] parents);

    void Unregister();
}
