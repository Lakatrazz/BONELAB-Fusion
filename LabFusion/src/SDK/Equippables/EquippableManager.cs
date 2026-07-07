using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.SDK.Messages;
using LabFusion.Utilities;

namespace LabFusion.SDK.Equippables;

public static class EquippableManager
{
    public static List<IEquippableItem> Equippables { get; } = new();

    public static Dictionary<string, IEquippableItem> BarcodeToEquippableLookup { get; } = new();

    public static HashSet<string> LocalEquippedItems { get; } = new();

    public static Dictionary<byte, HashSet<string>> NetEquippedItems { get; } = new();

    public static bool IsLocalEquipped(string barcode)
    {
        return LocalEquippedItems.Contains(barcode);
    }

    public static bool IsNetEquipped(PlayerID playerID, string barcode)
    {
        if (playerID.IsMe)
        {
            return IsLocalEquipped(barcode);
        }

        if (!NetEquippedItems.TryGetValue(playerID.SmallID, out var equippedItems))
        {
            return false;
        }

        return equippedItems.Contains(barcode);
    }

    public static IEquippableItem GetEquippable(string barcode) 
    {
        TryGetEquippable(barcode, out var equippable);
        return equippable;
    }

    public static void EquipEquippable(string barcode, bool equip = true) => ProcessLocalEquip(barcode, equip);

    public static bool TryGetEquippable(string barcode, out IEquippableItem equippable)
    {
        if (BarcodeToEquippableLookup.TryGetValue(barcode, out equippable))
        {
            return true;
        }

        equippable = null;
        return false;
    }

    public static void RegisterEquippable(IEquippableItem equippable)
    {
        Equippables.Add(equippable);

        BarcodeToEquippableLookup[equippable.Barcode] = equippable;

        InvokeAllEquipEvents(equippable);
    }

    internal static void Initialize()
    {
        MultiplayerHooking.OnJoinedServer += OnJoinedServer;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
    }

    internal static void ProcessLocalEquip(string barcode, bool equipped)
    {
        var equippedItems = LocalEquippedItems;

        if (equippedItems.Contains(barcode) == equipped)
        {
            return;
        }

        if (equipped)
        {
            equippedItems.Add(barcode);
        }
        else
        {
            equippedItems.Remove(barcode);
        }

        if (TryGetEquippable(barcode, out var equippable))
        {
            equippable.OnLocalEquipChanged(equipped);
        }

        SendEquippableEquip(barcode, equipped, CommonMessageRoutes.ReliableToOtherClients);
    }

    internal static void ProcessNetEquip(PlayerID playerID, string barcode, bool equipped)
    {
        var smallID = playerID.SmallID;

        if (!NetEquippedItems.ContainsKey(smallID))
        {
            NetEquippedItems[smallID] = new();
        }

        var equippedItems = NetEquippedItems[smallID];

        if (equippedItems.Contains(barcode) == equipped)
        {
            return;
        }

        if (equipped)
        {
            equippedItems.Add(barcode);
        }
        else
        {
            equippedItems.Remove(barcode);
        }

        if (TryGetEquippable(barcode, out var equippable))
        {
            equippable.OnNetEquipChanged(playerID, equipped);
        }
        else
        {
            DownloadEquippable(playerID, barcode);
        }
    }

    internal static void ClearNetEquippedItems(PlayerID playerID)
    {
        var smallID = playerID.SmallID;

        if (!NetEquippedItems.TryGetValue(smallID, out var equippedItems))
        {
            return;
        }

        foreach (var barcode in equippedItems)
        {
            if (!TryGetEquippable(barcode, out var equippable))
            {
                continue;
            }

            equippable.OnNetEquipChanged(playerID, false);
        }

        NetEquippedItems.Remove(smallID);
    }

    private static void OnJoinedServer()
    {
        SendAllEquippables(CommonMessageRoutes.ReliableToOtherClients);
    }

    private static void OnPlayerJoined(PlayerID playerID)
    {
        SendAllEquippables(new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }

    private static void SendAllEquippables(MessageRoute route)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        foreach (var barcode in LocalEquippedItems)
        {
            SendEquippableEquip(barcode, true, route);
        }
    }

    private static void SendEquippableEquip(string barcode, bool equipped, MessageRoute route)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = new EquippableEquipData()
        {
            Barcode = barcode,
            IsEquipped = equipped,
        };

        MessageRelay.RelayModule<EquippableEquipMessage, EquippableEquipData>(data, route);
    }

    private static void InvokeAllEquipEvents(IEquippableItem equippable)
    {
        var barcode = equippable.Barcode;

        if (IsLocalEquipped(barcode))
        {
            equippable.OnLocalEquipChanged(true);
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        foreach (var playerID in PlayerIDManager.PlayerIDs)
        {
            if (playerID.IsMe)
            {
                continue;
            }

            if (IsNetEquipped(playerID, barcode))
            {
                equippable.OnNetEquipChanged(playerID, true);
            }
        }
    }

    private static void DownloadEquippable(PlayerID playerID, string barcode)
    {
        bool shouldDownload = ClientSettings.Downloading.DownloadCosmetics.Value;

        if (!shouldDownload)
        {
            return;
        }

        long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxFileSize.Value);

        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            Target = playerID.SmallID,
            Barcode = barcode,
            MaxBytes = maxBytes,
        });
    }
}
