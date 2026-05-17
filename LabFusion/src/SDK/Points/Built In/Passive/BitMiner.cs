using LabFusion.Menu;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Points;

// TODO: Cleanup mess after point item system is cleaned up
[CompiledPointItem]
public class BitMiner : IPointItem
{
    public string Title => "Bit Miner";

    public string Author => "Lakatrazz";

    public string Description => CreateDescription(1);

    public int Price => 600;

    public string[] Tags => new string[2] {
        "Utility",
        "Passive",
    };

    public PointItemUpgrade[] Upgrades => new PointItemUpgrade[] {
        new(Description + CreateNextLevelDescription(1), 1000),
        new(CreateDescription(2) + CreateNextLevelDescription(2), 1200),
        new(CreateDescription(3) + CreateNextLevelDescription(3), 3000),
        new(CreateDescription(4) + CreateNextLevelDescription(4), 4200, CreateDescription(5) + "\n\nLevel: 4"),
    };

    private float _bitTime;

    private static string CreateNextLevelDescription(int level)
    {
        return $"\nNext: {level + 1} Bits/Player/Minute";
    }

    private static string CreateDescription(int bits)
    {
        string suffix = bits != 1 ? "s" : "";

        return $"<size=18>Hires NullMen to extract void energy from the depths of MythOS.\n\nCurrent: {bits} Bit{suffix}/Player/Minute";
    }

    private void OnLateUpdate()
    {
        if (!((IPointItem)this).IsUnlocked)
        {
            return;
        }

        if (!((IPointItem)this).IsEquipped)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!PlayerIDManager.HasOtherPlayers)
        {
            _bitTime = 0f;
            return;
        }

        _bitTime += TimeReferences.DeltaTime;

        if (_bitTime > 60f)
        {
            while (_bitTime > 60f)
            {
                _bitTime -= 60f;
                PointItemManager.RewardBits(CalculateBitReward(), false);
            }
        }
    }

    private int CalculateBitReward()
    {
        var baseCount = 2 + ((IPointItem)this).CurrentUpgradeIndex;

        var otherPlayers = PlayerIDManager.PlayerCount - 1;

        // Multiplicatively increase bits by player count
        var finalCount = baseCount * otherPlayers;

        return finalCount;
    }

    public void LoadIcon(Action<Texture2D> loadCallback)
    {
        loadCallback(MenuResources.GetPointIcon(Title).TryCast<Texture2D>());
    }

    public void OnRegistered()
    {
        MultiplayerHooking.OnLateUpdate += OnLateUpdate;
    }

    public void OnUnregistered()
    {
        MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
    }

    public void OnEquipChanged(PlayerID playerID, bool equipped)
    {
    }
}