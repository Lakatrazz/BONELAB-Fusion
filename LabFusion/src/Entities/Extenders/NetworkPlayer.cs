using Il2CppSLZ.Marrow.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable
{
    private NetworkEntity _networkEntity = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public MarrowEntity MarrowEntity => null;

    public NetworkPlayer(NetworkEntity entity)
    {
        _networkEntity = entity;

        entity.HookOnRegistered(OnPlayerRegistered);
        entity.OnEntityUnregistered += OnPlayerUnregistered;
    }

    private void OnPlayerRegistered(NetworkEntity entity)
    {
        entity.ConnectExtender(this);

        NetworkEntityManager.UpdateManager.Register(this);
        NetworkEntityManager.FixedUpdateManager.Register(this);
    }

    private void OnPlayerUnregistered(NetworkEntity entity)
    {
        entity.DisconnectExtender(this);
        _networkEntity = null;

        NetworkEntityManager.UpdateManager.Unregister(this);
        NetworkEntityManager.FixedUpdateManager.Unregister(this);
    }

    public void OnEntityUpdate(float deltaTime)
    {
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
    }
}
