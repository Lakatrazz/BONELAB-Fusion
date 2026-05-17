using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.SDK.Wearables;

public static class WearableManager
{
    public static WearableDisplayer LocalDisplayer { get; } = new();

    public static Dictionary<byte, WearableDisplayer> NetDisplayers { get; } = new();

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

        LocalPlayer.OnLocalRigCreated += OnLocalRigCreated;

        WearableItem.LocalEquipped += OnLocalEquipped;
        WearableItem.NetEquipped += OnNetEquipped;
    }

    private static void OnLocalRigCreated(RigManager rigManager)
    {
        LocalDisplayer.SetRigManager(rigManager);
    }

    private static void OnPlayerJoined(PlayerID playerID)
    {
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
        var smallID = playerID.SmallID;

        if (!NetDisplayers.TryGetValue(smallID, out var displayer))
        {
            displayer = new();
            NetDisplayers[smallID] = displayer;
        }

        if (equipped)
        {
            displayer.AddWearable(item);
        }
        else
        {
            displayer.RemoveWearable(item);
        }
    }
}
