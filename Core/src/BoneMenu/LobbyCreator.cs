using BoneLib.BoneMenu.Elements;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    public enum LobbySortMode {
        NONE = 0,
        GAMEMODE = 1,
        LEVEL = 2,
    }

    internal static partial class BoneMenuCreator
    {
        private static ulong _lobbyIndex = 0;

        public static void CreateLobby(MenuCategory category, LobbyMetadataInfo info, INetworkLobby lobby, LobbySortMode sortMode = LobbySortMode.NONE)
        {
            // Create the root category if necessary
            MenuCategory rootCategory = category;

            switch (sortMode) {
                case LobbySortMode.GAMEMODE:
                    rootCategory = category.CreateCategory(info.GamemodeName, Color.white);
                    break;
                case LobbySortMode.LEVEL:
                    rootCategory = category.CreateCategory(info.LevelName, Color.white);
                    break;
            }

            // Get the username/title of the lobby
            string userString;

            string countString = $"({info.PlayerCount}/{info.MaxPlayers})";

            if (!string.IsNullOrWhiteSpace(info.LobbyName)) {
                userString = $"{info.LobbyName} [{info.LobbyOwner}] {countString}";
            }
            else {
                userString = $"{info.LobbyOwner}'s Server {countString}";
            }

            // Change color based on version matching
            Color lobbyColor = Color.white;
            Color versionColor = Color.white;

            if (NetworkVerification.CompareVersion(info.LobbyVersion, FusionMod.Version) != VersionResult.Ok) {
                lobbyColor = Color.red;
                versionColor = Color.red;
            }
            else if (!info.ClientHasLevel)
                lobbyColor = Color.yellow;

            // Create the category and get the default lobby info
            var lobbyCategory = rootCategory.CreateCategory($"INTERNAL_LOBBY_{_lobbyIndex++}", lobbyColor);
            lobbyCategory.SetName(userString);

            lobbyCategory.CreateFunctionElement("Join Server", Color.white, info.CreateJoinDelegate(lobby));

            // Create a category for the player list
            var playersCategory = lobbyCategory.CreateCategory("Players", Color.white);

            foreach (var player in info.PlayerList.players) {
                playersCategory.CreateFunctionElement(player.username, Color.white, null);
            }

            RemoveEmptyCategory(lobbyCategory, playersCategory);

            // Create a category for the server tags
            var tagsCategory = lobbyCategory.CreateCategory("Tags", Color.white);

            foreach (var tag in info.LobbyTags.Expand()) {
                tagsCategory.CreateFunctionElement(tag, Color.white, null);
            }

            RemoveEmptyCategory(lobbyCategory, tagsCategory);

            // Allow outside mods to add their own lobby information
            var modsCategory = lobbyCategory.CreateCategory("Extra Info", Color.cyan);
            MultiplayerHooking.Internal_OnLobbyCategoryCreated(modsCategory, lobby);

            RemoveEmptyCategory(lobbyCategory, modsCategory);

            // Create general info panel
            var generalInfoPanel = lobbyCategory.CreateSubPanel("General Info", Color.white);

            // Names
            generalInfoPanel.CreateFunctionElement($"Username: {info.LobbyOwner}", Color.white, null);

            if (!string.IsNullOrWhiteSpace(info.LobbyName))
                generalInfoPanel.CreateFunctionElement($"Server Name: {info.LobbyName}", Color.white, null);

            // Show their version
            generalInfoPanel.CreateFunctionElement($"Version: {info.LobbyVersion}", versionColor, null);

            // Show their active level
            Color levelColor = info.ClientHasLevel ? Color.white : Color.red;

            generalInfoPanel.CreateFunctionElement($"Level: {info.LevelName}", levelColor, null);

            // Show the player count
            generalInfoPanel.CreateFunctionElement($"{info.PlayerCount} out of {info.MaxPlayers} Players", new Color(0.68f, 0.85f, 0.9f), null);

            // Show their active gamemode
            var gamemodePanel = lobbyCategory.CreateSubPanel("Gamemode Info", Color.white);
            gamemodePanel.CreateFunctionElement(info.GamemodeName, Color.white, null);
            gamemodePanel.CreateFunctionElement(info.IsGamemodeRunning ? "Running" : "Not Running", Color.white, null);

            // Create a category for settings
            var settingsCategory = lobbyCategory.CreateSubPanel("Settings", Color.yellow);

            settingsCategory.CreateFunctionElement($"Nametags: {(info.NametagsEnabled ? "Enabled" : "Disabled")}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Server Privacy: {info.Privacy}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Time Scale Mode: {info.TimeScaleMode}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Voicechat: {(info.VoicechatEnabled ? "Enabled" : "Disabled")}", Color.white, null);
        }
    }
}
