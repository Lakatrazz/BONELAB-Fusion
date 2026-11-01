using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Player;
using LabFusion.Network;

namespace LabFusion.Entities;

public static class NetworkPlayerManager
{
    public static EntityUpdatableManager UpdatableManager { get; } = new();

    public static bool HasExternalPlayer(RigManager rigManager)
    {
        if (!TryGetPlayer(rigManager, out var player))
        {
            return false;
        }

        return !player.NetworkEntity.IsOwner;
    }

    public static bool HasExternalPlayer(ClientSmallID playerID)
    {
        if (!TryGetPlayer(playerID, out var player))
        {
            return false;
        }

        return !player.NetworkEntity.IsOwner;
    }

    public static bool HasPlayer(RigManager rigManager)
    {
        return TryGetPlayer(rigManager, out _);
    }

    public static bool HasPlayer(byte playerID)
    {
        return NetworkEntityManager.IDManager.RegisteredEntities.HasEntity(new(playerID));
    }

    public static bool TryGetPlayer(ClientSmallID playerID, out NetworkPlayer player)
    {
        player = null;

        var entity = NetworkEntityManager.GetEntity(new((ushort)playerID));

        if (entity == null)
        {
            return false;
        }

        player = entity.GetExtender<NetworkPlayer>();
        return player != null;
    }

    public static bool TryGetPlayer(RigManager rigManager, out NetworkPlayer player)
    {
        if (NetworkRig.Cache.TryGet(rigManager, out var networkRig))
        {
            player = networkRig.NetworkEntity.GetExtender<NetworkPlayer>();
            return player != null;
        }

        player = null;
        return false;
    }

    public static bool TryGetPlayer(MarrowEntity marrowEntity, out NetworkPlayer player)
    {
        player = null;

        if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
        {
            return false;
        }

        player = networkEntity.GetExtender<NetworkPlayer>();
        return player != null;
    }

    internal static void Initialize()
    {
        ReserveIDs(PlayerIDManager.MinPlayerID, PlayerIDManager.MaxPlayerID);

        PlayerIDManager.PlayerRegistered += OnPlayerRegistered;
    }

    internal static void OnUpdate(float deltaTime) => UpdatableManager.OnEntityUpdate(deltaTime);
    internal static void OnFixedUpdate(float deltaTime) => UpdatableManager.OnEntityFixedUpdate(deltaTime);

    internal static void OnLateUpdate(float deltaTime) => UpdatableManager.OnEntityLateUpdate(deltaTime);

    private static void ReserveIDs(int min, int max)
    {
        for (var i = min; i <= max; i++)
        {
            NetworkEntityManager.IDManager.RegisteredEntities.Reserve(new((ushort)i));
        }
    }

    private static void OnPlayerRegistered(PlayerID playerID)
    {
        CreateNetworkPlayer(playerID);
    }

    private static NetworkPlayer CreateNetworkPlayer(PlayerID playerID)
    {
        NetworkEntity networkEntity = new();
        NetworkPlayer networkPlayer = NetworkPlayer.CreatePlayer(networkEntity, playerID);

        NetworkEntityManager.IDManager.RegisterEntity(new((ushort)playerID.SmallID), networkEntity);

        return networkPlayer;
    }
}
