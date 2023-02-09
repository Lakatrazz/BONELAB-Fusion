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
    internal static partial class BoneMenuCreator
    {
        public static void CreateLobby(MenuCategory category, LobbyMetadataInfo info, INetworkLobby lobby)
        {
            var userString = $"{info.LobbyName}'s Server ({info.PlayerCount}/{info.MaxPlayers})";

            // Create the category and get the default lobby info
            var lobbyCategory = category.CreateCategory(userString, Color.white);
            lobbyCategory.CreateFunctionElement("Join Server", Color.white, lobby.CreateJoinDelegate(info));

            // Show their active level
            lobbyCategory.CreateFunctionElement($"Level: {info.LevelName}", Color.white, null);

            // Show the player count
            lobbyCategory.CreateFunctionElement($"{info.PlayerCount} out of {info.MaxPlayers} Players", new Color(0.68f, 0.85f, 0.9f), null);

            // Create a category for settings
            var settingsCategory = lobbyCategory.CreateCategory("Settings", Color.yellow);

            settingsCategory.CreateFunctionElement($"Nametags: {(info.NametagsEnabled ? "Enabled" : "Disabled")}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Server Privacy: {info.Privacy}", Color.white, null);
            settingsCategory.CreateFunctionElement($"Time Scale Mode: {info.TimeScaleMode}", Color.white, null);

            // Allow outside mods to add their own lobby information
            MultiplayerHooking.Internal_OnLobbyCategoryCreated(lobbyCategory, lobby);
        }
    }
}
