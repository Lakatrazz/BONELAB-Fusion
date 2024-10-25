using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuProfile
{
    public static void PopulateProfile(GameObject profilePage)
    {
        // Update the profile grid
        var profilePanel = profilePage.transform.Find("panel_Profile").GetComponent<PlayerElement>();

        profilePanel.GetElements();

        profilePanel.UsernameElement
            .WithTitle(PlayerIdManager.LocalUsername);

        profilePanel.NicknameElement
            .WithTitle("Nickname")
            .AsPref(ClientSettings.Nickname, (value) =>
            {
                PlayerIdManager.LocalId?.Metadata.TrySetMetadata(MetadataHelper.NicknameKey, value);
            });

        profilePanel.DescriptionElement
            .WithTitle("Description")
            .AsPref(ClientSettings.Description);
    }
}