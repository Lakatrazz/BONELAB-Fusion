using UnityEngine;
using System.Collections.Generic;
using LabFusion.Representation;
using SLZ.VRMK;
using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.SDK.Gamemodes
{
    public class Team
    {
        public Team(string teamName, int maxPlayers = 6)
        {
            TeamName = teamName;
            TeamColor = Color.white;
            MaxPlayers = maxPlayers;

            Players = new List<PlayerId>();
            TeamAvatars = new List<Avatar>();
        }

        public Team(string teamName, Color teamColor, int maxPlayers = 6)
        {
            TeamName = teamName;
            TeamColor = teamColor;
            MaxPlayers = maxPlayers;

            Players = new List<PlayerId>();
            TeamAvatars = new List<Avatar>();
        }

        public string TeamName { get; }
        public Color TeamColor { get; }

        public Texture2D Logo { get; }
        public TeamLogo TeamLogo { get; }

        public int TeamScore { get; }

        public List<PlayerId> Players { get; }

        public int PlayerCount { get; private set; }
        public int MaxPlayers { get; }

        public AudioClip WinMusic { get; private set; }
        public AudioClip LossMusic { get; private set; }

        public List<Avatar> TeamAvatars { get; private set; }

        public void AddPlayer(PlayerId playerId)
        {
            if(PlayerCount > MaxPlayers)
            {
                return;
            }

            Players.Add(playerId);
        }

        public void RemovePlayer(PlayerId playerId)
        {
            if(PlayerCount <= 0)
            {
                return;
            }

            Players.Remove(playerId);
            PlayerCount--;
        }
    }
}
