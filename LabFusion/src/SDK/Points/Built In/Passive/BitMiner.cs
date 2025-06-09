using LabFusion.Menu;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Points;

[CompiledPointItem]
public class BitMiner : PointItem
{
    public override string Title => "Bit Miner";

    public override string Author => "Lakatrazz";

    public override string Description => CreateDescription(1);

    public override int Price => 600;

    public override string[] Tags => new string[2] {
        "Utility",
        "Passive",
    };

    public override PointItemUpgrade[] Upgrades => new PointItemUpgrade[] {
        new(Description + CreateNextLevelDescription(1), 1000),
        new(CreateDescription(2) + CreateNextLevelDescription(2), 1200),
        new(CreateDescription(3) + CreateNextLevelDescription(3), 3000),
        new(CreateDescription(4) + CreateNextLevelDescription(4), 4200, CreateDescription(5) + "\n\nLevel: 4"),
    };

    public override bool ImplementLateUpdate => true;

    private float _bitTime;

    private static string CreateNextLevelDescription(int level)
    {
        return $"\n\nNext Level: {level} - Grants {level + 1} bits per minute.";
    }

    private static string CreateDescription(int bits)
    {
        string suffix = bits != 1 ? "s" : "";

        return $"Hires a team of hard working nullbodies to mine valuables from the depths of MythOS. Grants {bits} bit{suffix} for every other player per minute you are in a Fusion lobby.";
    }

    public override void OnLateUpdate()
    {
        if (!IsUnlocked)
        {
            return;
        }

        if (!IsEquipped)
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

        _bitTime += TimeUtilities.DeltaTime;

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
        var baseCount = 2 + CurrentUpgradeIndex;

        var otherPlayers = PlayerIDManager.PlayerCount - 1;

        // Multiplicatively increase bits by player count
        var finalCount = baseCount * otherPlayers;

        return finalCount;
    }

    public override void LoadPreviewIcon(Action<Texture2D> onLoaded)
    {
        onLoaded(MenuResources.GetPointIcon(Title).TryCast<Texture2D>());
    }
}