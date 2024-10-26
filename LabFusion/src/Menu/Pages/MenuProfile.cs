using LabFusion.Data;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuProfile
{
    public static PlayerElement ProfileElement { get; private set; }

    private static void UpdatePlayerIcon(PlayerElement element)
    {
        var avatarTitle = RigData.Refs.RigManager.AvatarCrate.Crate.Title;

        var avatarIcon = MenuResources.GetAvatarIcon(avatarTitle);

        if (avatarIcon == null)
        {
            avatarIcon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        element.PlayerIcon.texture = avatarIcon;
    }

    public static void RefreshIcon()
    {
        UpdatePlayerIcon(ProfileElement);
    }

    public static void PopulateProfile(GameObject profilePage)
    {
        // Update the profile grid
        ProfileElement = profilePage.transform.Find("panel_Profile").GetComponent<PlayerElement>();

        ProfileElement.GetElements();

        ProfileElement.UsernameElement
            .WithTitle(PlayerIdManager.LocalUsername);

        ProfileElement.NicknameElement
            .WithTitle("Nickname")
            .AsPref(ClientSettings.Nickname, (value) =>
            {
                PlayerIdManager.LocalId?.Metadata.TrySetMetadata(MetadataHelper.NicknameKey, value);
            });

        ProfileElement.DescriptionElement
            .WithTitle("Description")
            .AsPref(ClientSettings.Description);

        // Update the icon
        ProfileElement.PlayerPage.OnShown += () =>
        {
            RefreshIcon();
        };

        RefreshIcon();

        // Disable unnecessary elements
        ProfileElement.ActionsGrid.SetActive(false);

        ProfileElement.PermissionsElement.gameObject.SetActive(false);
    }
}