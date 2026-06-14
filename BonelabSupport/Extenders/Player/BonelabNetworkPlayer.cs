using LabFusion.Entities;

namespace MarrowFusion.Bonelab.Extenders;

public class BonelabNetworkPlayer : IEntityExtender, IPlayerLateUpdatable
{
    public NetworkEntity NetworkEntity { get; private set; } = null;

    public NetworkPlayer NetworkPlayer { get; private set; } = null;

    public BonelabRigVitals RigVitals { get; } = new();

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
        NetworkEntity.ConnectExtender(this);
    }

    public void OnExtenderRegistered()
    {
        NetworkPlayer.UpdatableManager.LateUpdateManager.Register(this);
    }

    public void OnExtenderUnregistered()
    {
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

        RigVitals.Resolve(NetworkPlayer.RigRefs);
    }
}
