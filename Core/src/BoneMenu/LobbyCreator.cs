using BoneLib.BoneMenu.Elements;

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
            var userString = $"{info.LobbyName}'s Server ({info.PlayerCount}/{info.MaxPlayers})";

            // Change color based on version matching
            Color lobbyColor = Color.white;

            if (NetworkVerification.CompareVersion(info.LobbyVersion, FusionMod.Version) != VersionResult.Ok)
                lobbyColor = Color.red;

            // Create the category and get the default lobby info
            var lobbyCategory = rootCategory.CreateCategory($"INTERNAL_LOBBY_{_lobbyIndex++}", lobbyColor);
            lobbyCategory.SetName(userString);

            lobbyCategory.CreateFunctionElement("Join Server", Color.white, lobby.CreateJoinDelegate(info));

            // Show their version
            lobbyCategory.CreateFunctionElement($"Version: {info.LobbyVersion}", lobbyColor, null);

            // Show their active level
            lobbyCategory.CreateFunctionElement($"Level: {info.LevelName}", Color.white, null);

            // Show their active gamemode
            lobbyCategory.CreateFunctionElement($"Gamemode: {info.GamemodeName}", Color.white, null);

            // Show the player count
            lobbyCategory.CreateFunctionElement($"{info.PlayerCount} out of {info.MaxPlayers} Players", new Color(0.68f, 0.85f, 0.9f), null);

            // Create a category for settings
            var settingsCategory = lobbyCategory.CreateCategory("Settings", Color.yellow);

            settingsCategory.CreateFunctionElement($"Nametags: {(info.NametagsEnabled ? "Enabled" : "Disabled")}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Server Privacy: {info.Privacy}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Time Scale Mode: {info.TimeScaleMode}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Voicechat: {(info.VoicechatEnabled ? "Enabled" : "Disabled")}", Color.white, null);

            // Allow outside mods to add their own lobby information
            MultiplayerHooking.Internal_OnLobbyCategoryCreated(lobbyCategory, lobby);
        }
    }
}
