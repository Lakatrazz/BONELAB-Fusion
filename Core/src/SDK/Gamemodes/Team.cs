using UnityEngine;

namespace LabFusion.SDK.Gamemodes
{
    public class Team
    {
        public Team(string teamName, int maxPlayers = 6)
        {
            TeamName = teamName;
            TeamColor = Color.white;
            MaxPlayers = maxPlayers;
        }

        public Team(string teamName, Color teamColor, int maxPlayers = 6)
        {
            TeamName = teamName;
            TeamColor = teamColor;
            MaxPlayers = maxPlayers;
        }

        public string TeamName { get; }
        public Color TeamColor { get; }

        public Texture2D Logo { get; }
        public TeamLogo TeamLogo { get; }

        public int TeamScore { get; }

        public int PlayerCount { get; }
        public int MaxPlayers { get; }

        public AudioClip WinMusic { get; private set; }
        public AudioClip LossMusic { get; private set; }
    }
}
