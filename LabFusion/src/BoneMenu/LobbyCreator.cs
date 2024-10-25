using BoneLib.BoneMenu;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.SDK.Lobbies;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    public enum LobbySortMode
    {
        NONE = 0,
        GAMEMODE = 1,
        LEVEL = 2,
    }

    public static partial class BoneMenuCreator
    {
        private static ulong _lobbyIndex = 0;

        private static Page _filterCategory = null;

        public static void CreateFilters(Page page)
        {
            _filterCategory = page.CreatePage("Filters", Color.white);

            foreach (var filter in LobbyFilterManager.LobbyFilters)
            {
                AddFilter(filter);
            }
        }

        private static void AddFilter(ILobbyFilter filter)
        {
            _filterCategory.CreateBool(filter.GetTitle(), Color.white, filter.IsActive(), (v) =>
            {
                filter.SetActive(v);
            });
        }

        public static void CreateLobby(Page page, LobbyMetadataInfo info, INetworkLobby lobby, LobbySortMode sortMode = LobbySortMode.NONE)
        {
            // Create the root category if necessary
            Page rootCategory = page;

            switch (sortMode)
            {
                case LobbySortMode.GAMEMODE:
                    rootCategory = page.CreatePage(info.GamemodeName, Color.white);
                    break;
                case LobbySortMode.LEVEL:
                    rootCategory = page.CreatePage(info.LevelName, Color.white);
                    break;
            }

            // Get the username/title of the lobby
            string userString;

            string countString = $"({info.PlayerCount}/{info.MaxPlayers})";

            if (!string.IsNullOrWhiteSpace(info.LobbyName))
            {
                userString = $"{info.LobbyName} [{info.LobbyOwner}] {countString}";
            }
            else
            {
                userString = $"{info.LobbyOwner}'s Server {countString}";
            }

            // Change color based on version matching
            Color lobbyColor = Color.white;
            Color versionColor = Color.white;

            if (NetworkVerification.CompareVersion(info.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
            {
                lobbyColor = Color.red;
                versionColor = Color.red;
            }
            else if (!info.ClientHasLevel)
            {
                lobbyColor = Color.yellow;
            }

            // Create the category and get the default lobby info
            var lobbyCategory = rootCategory.CreatePage($"INTERNAL_LOBBY_{_lobbyIndex++}", lobbyColor);
            lobbyCategory.Name = userString;

            lobbyCategory.CreateFunction("Join Server", Color.white, info.CreateJoinDelegate(lobby));

            // Create a category for the player list
            var playersCategory = lobbyCategory.CreatePage("Players", Color.white, 0, false);
            var playersLink = lobbyCategory.CreatePageLink(playersCategory);

            foreach (var player in info.PlayerList.Players)
            {
                playersCategory.CreateFunction(player.Username, Color.white, null);
            }
            
            RemoveEmptyPage(lobbyCategory, playersCategory, playersLink);

            // Create a category for the server tags
            var tagsCategory = lobbyCategory.CreatePage("Tags", Color.white, 0, false);
            var tagsLink = lobbyCategory.CreatePageLink(tagsCategory);

            foreach (var tag in info.LobbyTags.Expand())
            {
                tagsCategory.CreateFunction(tag, Color.white, null);
            }

            RemoveEmptyPage(lobbyCategory, tagsCategory, tagsLink);

            // Allow outside mods to add their own lobby information
            var modsCategory = lobbyCategory.CreatePage("Extra Info", Color.cyan, 0, false);
            var modsLink = lobbyCategory.CreatePageLink(modsCategory);

            MultiplayerHooking.Internal_OnLobbyCategoryCreated(modsCategory, lobby);

            RemoveEmptyPage(lobbyCategory, modsCategory, modsLink);

            // Create general info panel
            var generalInfoPanel = lobbyCategory.CreatePage("General Info", Color.white);

            // Names
            generalInfoPanel.CreateFunction($"Username: {info.LobbyOwner}", Color.white, null);

            if (!string.IsNullOrWhiteSpace(info.LobbyName))
            {
                generalInfoPanel.CreateFunction($"Server Name: {info.LobbyName}", Color.white, null);
            }

            // Show their version
            generalInfoPanel.CreateFunction($"Version: {info.LobbyVersion}", versionColor, null);

            // Show their active level
            Color levelColor = info.ClientHasLevel ? Color.white : Color.red;

            generalInfoPanel.CreateFunction($"Level: {info.LevelName}", levelColor, null);

            // Show the player count
            generalInfoPanel.CreateFunction($"{info.PlayerCount} out of {info.MaxPlayers} Players", new Color(0.68f, 0.85f, 0.9f), null);

            // Show their active gamemode
            var gamemodePanel = lobbyCategory.CreatePage("Gamemode Info", Color.white);
            gamemodePanel.CreateFunction(info.GamemodeName, Color.white, null);
            gamemodePanel.CreateFunction(info.IsGamemodeRunning ? "Running" : "Not Running", Color.white, null);

            // Create a category for settings
            var settingsCategory = lobbyCategory.CreatePage("Settings", Color.yellow);

            settingsCategory.CreateFunction($"Nametags: {(info.NametagsEnabled ? "Enabled" : "Disabled")}", Color.white, null);
            settingsCategory.CreateFunction($"Server Privacy: {info.Privacy}", Color.white, null);
            settingsCategory.CreateFunction($"Time Scale Mode: {info.TimeScaleMode}", Color.white, null);
            settingsCategory.CreateFunction($"Voicechat: {(info.VoiceChatEnabled ? "Enabled" : "Disabled")}", Color.white, null);
        }
    }
}
