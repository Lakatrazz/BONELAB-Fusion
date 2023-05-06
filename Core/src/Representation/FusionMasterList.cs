using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace LabFusion.Representation {
    public enum FusionMasterResult {
        NORMAL = 1 << 0,
        IMPERSONATOR = 1 << 1,
        MASTER = 1 << 2,
    }

    public static class FusionMasterList {
        public struct MasterPlayer {
            public ulong id;
            public string name;
            public bool unique;

            public MasterPlayer(ulong id, string name, bool unique = true) {
                this.id = id;
                this.name = name;
                this.unique = unique;
            }
        }

        private static readonly MasterPlayer[] _steamPlayers = new MasterPlayer[] {
            new MasterPlayer(76561198198752494, "Lakatrazz"),
            new MasterPlayer(76561198097630377, "AlexTheBaBa"),
            new MasterPlayer(76561198222917852, "Mr.Gaming"),
            new MasterPlayer(76561198096586464, "brwok"),
            new MasterPlayer(76561198143565238, "Riggle"),
            new MasterPlayer(76561198233973112, "Alfie"),
            new MasterPlayer(76561198061847729, "zz0000"),
            new MasterPlayer(76561198837064193, "172", false),
        };

        public static FusionMasterResult VerifyPlayer(ulong id, string name) {
            if (NetworkInfo.CurrentNetworkLayer is SteamNetworkLayer)
                return Internal_VerifyPlayer(_steamPlayers, id, name);
            
            return FusionMasterResult.NORMAL;
        }

        private static FusionMasterResult Internal_VerifyPlayer(MasterPlayer[] players, ulong id, string name) {
            for (var i = 0; i < players.Length; i++) {
                var player = players[i];

                // Our id matches, and this is a master user
                if (player.id == id) {
                    return FusionMasterResult.MASTER;
                }

                // Convert names to have no whitespace and in lowercase
                string masterName = Regex.Replace(player.name, @"\s+", "").ToLower();
                string otherName = Regex.Replace(name, @"\s+", "").ToLower();

                // The name matches, but the id didn't
                if (masterName == otherName && player.unique) {
                    return FusionMasterResult.IMPERSONATOR;
                }
            }

            // Neither name nor id matched, this is a regular joe
            return FusionMasterResult.NORMAL;
        }
    }
}
