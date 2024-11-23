using LabFusion.Marrow.Proxies;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuLogIn
{
    public static void PopulateLogIn(GameObject logInGameObject)
    {
        // Layer Panel
        var layerPanel = logInGameObject.transform.Find("panel_Layer");

        var layoutOptions = layerPanel.Find("layout_Options");

        var targetLayerLabel = layoutOptions.Find("label_TargetLayer").GetComponent<LabelElement>()
            .WithTitle("Target Layer");

        var cycleLayerElement = layoutOptions.Find("button_CycleLayer").GetComponent<FunctionElement>()
            .WithTitle("Cycle")
            .Do(() =>
            {

            });

        var logInElement = layoutOptions.Find("button_LogIn").GetComponent<FunctionElement>()
            .WithTitle("Log In")
            .Do(() =>
            {

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
    }
}
