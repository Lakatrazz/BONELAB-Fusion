using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.SDK.Wearables;

public static class WearableManager
{
    public static WearableDisplayer LocalDisplayer { get; } = new();

    public static Dictionary<ClientSmallID, WearableDisplayer> NetDisplayers { get; } = new();

    public static WearableDisplayer GetWearableDisplayer(RigManager rigManager)
    {
        if (rigManager.IsLocalPlayer())
        {
            return LocalDisplayer;
        }

        if (NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
        {
            var smallID = player.PlayerID.SmallID;

            if (!NetDisplayers.TryGetValue(smallID, out var netDisplayer))
            {
                netDisplayer = new();
                NetDisplayers[smallID] = netDisplayer;
            }

            return netDisplayer;
        }

        return null;
    }

    internal static void Initialize()
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
        MultiplayerHooking.OnDisconnected += OnDisconnected;

        LocalPlayer.OnLocalRigCreated += OnLocalRigCreated;
        NetworkPlayer.OnNetworkRigCreated += OnNetRigCreated;

        WearableItem.LocalEquipped += OnLocalEquipped;
        WearableItem.NetEquipped += OnNetEquipped;
    }

    private static void OnLocalRigCreated(RigManager rigManager)
    {
        LocalDisplayer.SetRigManager(rigManager);
    }

    private static void OnNetRigCreated(NetworkPlayer player, RigManager rigManager)
    {
        if (player.NetworkEntity.IsOwner)
        {
            return;
        }

        var displayer = GetOrAddNetDisplayer(player.PlayerID);
        displayer.SetRigManager(rigManager);
    }

    private static void OnPlayerJoined(PlayerID playerID)
    {
        GetOrAddNetDisplayer(playerID);
    }

    private static void OnPlayerLeft(PlayerID playerID)
    {
        ClearNetDisplayer(playerID.SmallID);
    }

    private static void OnDisconnected()
    {
        ClearNetDisplayers();
    }

    private static void OnLocalEquipped(WearableItem item, bool equipped)
    {
        if (equipped)
        {
            LocalDisplayer.AddWearable(item);
        }
        else
        {
            LocalDisplayer.RemoveWearable(item);
        }
    }

    private static void OnNetEquipped(WearableItem item, PlayerID playerID, bool equipped)
    {
        var displayer = GetOrAddNetDisplayer(playerID);

        if (equipped)
        {
            displayer.AddWearable(item);
        }
        else
        {
            displayer.RemoveWearable(item);
        }
    }

    private static WearableDisplayer GetOrAddNetDisplayer(PlayerID playerID)
    {
        var smallID = playerID.SmallID;

        if (NetDisplayers.TryGetValue(smallID, out var netDisplayer))
        {
            return netDisplayer;
        }

        var displayer = new WearableDisplayer()
        {
            PlayerID = playerID,
        };
        NetDisplayers[smallID] = displayer;

        return displayer;
    }

    private static void ClearNetDisplayer(ClientSmallID smallID)
    {
        if (NetDisplayers.TryGetValue(smallID, out var displayer))
        {
            displayer.ClearRigManager();
        }

        NetDisplayers.Remove(smallID);
    }

    private static void ClearNetDisplayers()
    {
        foreach (var displayer in NetDisplayers.Values)
        {
            displayer.ClearRigManager();
        }

        NetDisplayers.Clear();
    }
}
