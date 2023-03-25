using UnityEngine;

using System.Collections.Generic;

namespace LabFusion.SDK.Gamemodes
{
    public class Team
    {
        public Team(string teamName, Color teamColor, int maxPlayers)
        {
            TeamName = teamName;
            TeamColor = teamColor;
            MaxPlayers = maxPlayers;
        }

        public string TeamName { get; }
        public Color TeamColor { get; }
        public int TeamScore { get; }
        public int PlayerCount { get; }
        public int MaxPlayers { get; }
    }
}
