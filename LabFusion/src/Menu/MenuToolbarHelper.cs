using LabFusion.Marrow.Proxies;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuToolbarHelper
{
    public static void PopulateToolbar(GameObject toolbar)
    {
        // Setup the mute button
        var muteButton = toolbar.transform.Find("button_Muted").GetComponent<BoolElement>();

        muteButton.AsPref(ClientSettings.VoiceChat.Muted);
    }
}
