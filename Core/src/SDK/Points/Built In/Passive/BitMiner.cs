using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class BitMiner : PointItem
    {
        public override string Title => "Bit Miner";

        public override string Author => "Lakatrazz";

        public override string Description => Internal_CreateDescription(1);

        public override RarityLevel Rarity => RarityLevel.Purple;

        public override int Price => 600;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BitMiner)).Preview;

        public override string[] Tags => new string[2] {
            "Utility",
            "Passive",
        };

        public override PointItemUpgrade[] Upgrades => new PointItemUpgrade[] {
            new PointItemUpgrade(Description + Internal_CreateNextLevelDescription(1), 1000),
            new PointItemUpgrade(Internal_CreateDescription(2) + Internal_CreateNextLevelDescription(2), 1200),
            new PointItemUpgrade(Internal_CreateDescription(3) + Internal_CreateNextLevelDescription(3), 3000),
            new PointItemUpgrade(Internal_CreateDescription(4) + Internal_CreateNextLevelDescription(4), 4200, Internal_CreateDescription(5) + "\n\nLevel: 4"),
        };

        public override bool ImplementLateUpdate => true;

        private float _bitTime;

        private static string Internal_CreateNextLevelDescription(int level)
        {
            return $"\n\nNext Level: {level} - Grants {level + 1} bits per minute.";
        }

        private static string Internal_CreateDescription(int bits)
        {
            string suffix = bits != 1 ? "s" : "";

            return $"Hires a team of hard working nullbodies to mine valuables from the depths of MythOS. Grants {bits} bit{suffix} per minute you are in a Fusion lobby with another person.";
        }

        public override void OnLateUpdate()
        {
            if (IsUnlocked && IsEquipped && NetworkInfo.HasServer)
            {
                if (PlayerIdManager.HasOtherPlayers)
                {
                    _bitTime += TimeUtilities.DeltaTime;

                    if (_bitTime > 60f)
                    {
                        while (_bitTime > 60f)
                        {
                            _bitTime -= 60f;
                            PointItemManager.RewardBits(1 + (UpgradeLevel + 1), false);
                        }
                    }
                }
                else
                {
                    _bitTime = 0f;
                }
            }
        }
    }
}
