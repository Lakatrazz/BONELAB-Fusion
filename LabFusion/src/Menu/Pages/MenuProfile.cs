using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Player;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuProfile
{
    public static PlayerElement ProfileElement { get; private set; }

    private static string PreviousAvatarTitle { get; set; } = null;

    public static void OnInitializeMelon()
    {
        LocalPlayer.OnUsernameChanged += (v) =>
        {
            RefreshName();
        };
    }

    private static void UpdatePlayerIcon(PlayerElement element)
    {
        var avatarCrate = RigData.Refs.RigManager.AvatarCrate.Crate;
        var avatarTitle = avatarCrate.Title;

        // No need to update the icon if its the same avatar
        if (avatarTitle == PreviousAvatarTitle)
        {
            return;
        }

        PreviousAvatarTitle = avatarTitle;

        ElementIconHelper.SetProfileIcon(element, avatarTitle, CrateFilterer.GetModID(avatarCrate.Pallet));
    }

    private static void RefreshIcon()
    {
        UpdatePlayerIcon(ProfileElement);
    }

    private static void RefreshName()
    {
        if (ProfileElement == null)
        {
            return;
        }

        ProfileElement.UsernameElement
            .WithTitle(LocalPlayer.Username);
    }

    public static void PopulateProfile(GameObject profilePage)
    {
        // Reset the stored avatar
        PreviousAvatarTitle = null;

        // Update the profile grid
        ProfileElement = profilePage.transform.Find("panel_Profile").GetComponent<PlayerElement>();

        ProfileElement.GetElements();

        ProfileElement.UsernameElement
            .WithTitle(LocalPlayer.Username);

        ProfileElement.NicknameElement
            .WithTitle("Nickname")
            .AsPref(ClientSettings.Nickname);

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
        ProfileElement.OptionsGameObject.SetActive(false);
        ProfileElement.ActionsGrid.SetActive(false);
    }
}