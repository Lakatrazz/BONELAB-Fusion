using LabFusion.Entities;

namespace LabFusion.Bonelab.Extenders;

public class BonelabNetworkPlayer : IEntityExtender, IPlayerLateUpdatable
{
    public NetworkEntity NetworkEntity { get; private set; } = null;

    public NetworkPlayer NetworkPlayer { get; private set; } = null;

    public static BonelabNetworkPlayer CreatePlayer(NetworkEntity networkEntity, NetworkPlayer networkPlayer)
    {
        var bonelabPlayer = new BonelabNetworkPlayer(networkEntity, networkPlayer);

        bonelabPlayer.Initialize();

        return bonelabPlayer;
    }

    private BonelabNetworkPlayer(NetworkEntity networkEntity, NetworkPlayer networkPlayer)
    {
        NetworkEntity = networkEntity;
        NetworkPlayer = networkPlayer;
    }

    private void Initialize()
    {
        NetworkEntity.HookOnRegistered(OnBonelabPlayerRegistered);
        NetworkEntity.OnEntityUnregistered += OnBonelabPlayerUnregistered;
    }

    private void OnBonelabPlayerRegistered(NetworkEntity entity)
    {
        entity.ConnectExtender(this);

        NetworkPlayer.UpdatableManager.LateUpdateManager.Register(this);
    }

    private void OnBonelabPlayerUnregistered(NetworkEntity entity)
    {
        entity.DisconnectExtender(this);

        NetworkPlayer.UpdatableManager.LateUpdateManager.Unregister(this);
    }

    public void OnPlayerLateUpdate(float deltaTime)
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        if (!NetworkPlayer.HasRig)
        {
            return;
        }

        // TODO: Process BodyVitals here
    }
}
