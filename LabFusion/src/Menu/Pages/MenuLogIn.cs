using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuLogIn
{
    public static FunctionElement ChangeLayerElement { get; private set; } = null;

    private static int _lastLayerIndex = -1;

    private static void OnLogInPageShown()
    {
        ChangeLayerElement.gameObject.SetActive(NetworkLayer.SupportedLayers.Count > 1);
    }

    public static void PopulateLogIn(GameObject logInGameObject)
    {
        var logInPage = logInGameObject.GetComponent<MenuPage>();
        logInPage.OnShown += OnLogInPageShown;

        // Layer Panel
        var layerPanel = logInGameObject.transform.Find("panel_Layer");

        var layoutOptions = layerPanel.Find("layout_Options");

        var targetLayerLabel = layoutOptions.Find("label_TargetLayer").GetComponent<LabelElement>()
            .WithTitle($"Target Layer: {ClientSettings.NetworkLayerTitle.Value}");

        ChangeLayerElement = layoutOptions.Find("button_CycleLayer").GetComponent<FunctionElement>()
            .WithTitle("Change Layer")
            .Do(() =>
            {
                int count = NetworkLayer.SupportedLayers.Count;

                if (count <= 0)
                {
                    return;
                }

                _lastLayerIndex++;

                if (count <= _lastLayerIndex)
                {
                    _lastLayerIndex = 0;
                }

                ClientSettings.NetworkLayerTitle.Value = NetworkLayer.SupportedLayers[_lastLayerIndex].Title;
            });

        ClientSettings.NetworkLayerTitle.OnValueChanged += (v) =>
        {
            targetLayerLabel.Title = $"Target Layer: {v}";
        };

        var logInElement = layoutOptions.Find("button_LogIn").GetComponent<FunctionElement>()
            .WithTitle("Log In")
            .Do(() =>
            {
                var layer = NetworkLayerManager.GetTargetLayer();

                if (layer != null)
                {
                    NetworkLayerManager.LogIn(layer);
                }
            });

        // Connecting Panel
        var connectingPanel = logInGameObject.transform.Find("panel_Connecting");

        connectingPanel.Find("label_Connecting").GetComponent<LabelElement>()
            .WithTitle("Connecting");

        // Failed Panel
        var failedPanel = logInGameObject.transform.Find("panel_Failed");

        failedPanel.Find("label_Failed").GetComponent<LabelElement>()
            .WithTitle("Connection Failed");

        // Success Panel
        var successPanel = logInGameObject.transform.Find("panel_Success");

        successPanel.Find("label_Success").GetComponent<LabelElement>()
            .WithTitle("Connection Succeeded");

        OnLogInPageShown();
    }
}
