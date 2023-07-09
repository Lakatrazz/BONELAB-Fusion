using UnityEngine;
using System.Collections.Generic;
using LabFusion.Representation;
using SLZ.VRMK;
using Avatar = SLZ.VRMK.Avatar;
using BoneLib;

namespace LabFusion.SDK.Gamemodes
{
    public class Team
    {
        public Team()
        {

        }

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
            _displayName = null;
            TeamColor = teamColor;
            MaxPlayers = maxPlayers;

            Players = new List<PlayerId>();
            TeamAvatars = new List<Avatar>();
        }

        public string TeamName { get; }
        public Color TeamColor { get; }

        public Texture2D Logo { get; private set; }
        public List<TeamLogoInstance> LogoInstances { get; private set; }

        private string _displayName = null;
        public string DisplayName => _displayName ?? TeamName;

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

            ConstructLogoInstance(playerId);
        }

        public void RemovePlayer(PlayerId playerId)
        {
            if(PlayerCount <= 0)
            {
                return;
            }

            Players.Remove(playerId);
            PlayerCount--;

            TeamLogoInstance playerLogo = LogoInstances.Find((logo) => logo.playerId == playerId);
            LogoInstances.Remove(playerLogo);
        }

        public void SetLogo(Texture2D logo)
        {
            this.Logo = logo;
        }

        public void SetDisplayName(string displayName)
        {
            this._displayName = displayName;
        }

        public void SetMusic(AudioClip winMusic = null, AudioClip lossMusic = null)
        {
            WinMusic = winMusic;
            LossMusic = lossMusic;
        }

        private void ConstructLogoInstance(PlayerId playerId)
        {
            if (LogoInstances == null)
            {
                LogoInstances = new List<TeamLogoInstance>();
            }

            TeamLogoInstance logoInstance = new TeamLogoInstance(playerId, this);
            LogoInstances.Add(logoInstance);
        }
    }
}
