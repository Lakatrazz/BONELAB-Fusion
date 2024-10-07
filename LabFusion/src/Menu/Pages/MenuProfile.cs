using Il2CppTMPro;

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
        var gridPofile = profilePage.transform.Find("grid_Profile");

        var usernameLabel = gridPofile.Find("label_Username");
        usernameLabel.Find("text").GetComponent<TMP_Text>().text = PlayerIdManager.LocalUsername;

        var nicknameButton = gridPofile.Find("button_Nickname").GetComponent<StringElement>()
            .AsPref(ClientSettings.Nickname, (value) =>
            {
                PlayerIdManager.LocalId?.Metadata.TrySetMetadata(MetadataHelper.NicknameKey, value);
            });

        nicknameButton.Title = "Nickname";

        var descriptionButton = gridPofile.Find("button_Description").GetComponent<StringElement>()
            .AsPref(ClientSettings.Description);
        descriptionButton.Title = "Description";
    }
}