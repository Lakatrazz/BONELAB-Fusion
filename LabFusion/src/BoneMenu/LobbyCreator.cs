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
        }
    }
}
