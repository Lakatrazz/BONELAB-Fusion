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

        public override string Description => "Hires a team of hard working nullbodies to mine valuables from the depths of MythOS. Grants 1 bit per minute you are in a Fusion lobby with another person.";

        public override RarityLevel Rarity => RarityLevel.Purple;

        public override int Price => 600;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BitMiner)).Preview;

        public override string[] Tags => new string[2] {
            "Utility",
            "Passive",
        };

        public override bool ImplementLateUpdate => true;

        private float _bitTime;

        public override void OnLateUpdate() {
            if (IsUnlocked && IsEquipped && NetworkInfo.HasServer) {
                if (PlayerIdManager.PlayerCount > 1) {
                    _bitTime += Time.deltaTime;

                    if (_bitTime > 60f) {
                        while (_bitTime > 60f) {
                            _bitTime -= 60f;
                            PointItemManager.RewardBits(1);
                        }
                    }
                }
                else {
                    _bitTime = 0f;
                }
            }
        }
    }
}
