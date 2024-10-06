using Il2CppSLZ.Bonelab;
using Il2CppTMPro;

using LabFusion.Marrow.Proxies;
using LabFusion.Preferences;
using LabFusion.Utilities;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuButtonHelper
{
    public static void SetBoolPref(BoolElement button, FusionPref<bool> pref, Action<bool> onValueChanged = null)
    {
        button.Value = pref.Value;

        var onButtonChanged = (bool value) =>
        {
            pref.Value = value;
        };
        button.OnValueChanged += onButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (button.Value != value)
            {
                button.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        button.OnDestroyed += () =>
        {
            button.OnValueChanged -= onButtonChanged;
        };
    }

    public static void SetStringPref(StringElement button, FusionPref<string> pref, Action<string> onValueChanged = null)
    {
        button.Value = pref.Value;

        var onButtonChanged = (string value) =>
        {
            pref.Value = value;
        };
        button.OnValueChanged += onButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (button.Value != value)
            {
                button.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        button.OnDestroyed += () =>
        {
            button.OnValueChanged -= onButtonChanged;
        };
    }

    public static void PopulateTexts(GameObject root)
    {
        var texts = root.GetComponentsInChildren<TMP_Text>(true);

        foreach (var text in texts)
        {
            text.font = PersistentAssetCreator.Font;
        }
    }

    public static void PopulateButtons(GameObject root)
    {
        var controlFeedback = UIRig.Instance.transform.Find("DATAMANAGER/CONTROL_FEEDBACK");
        var tacticle = controlFeedback.GetComponent<Feedback_Tactile>();
        var audio = controlFeedback.GetComponent<Feedback_Audio>();

        var buttons = root.GetComponentsInChildren<Button>(true);

        foreach (var button in buttons)
        {
            PopulateButton(button, tacticle, audio);
        }
    }

    public static void PopulateButton(Button button, Feedback_Tactile tacticle, Feedback_Audio audio)
    {
        var hoverClick = button.gameObject.AddComponent<ButtonHoverClick>();

        hoverClick.feedback_tactile = tacticle;
        hoverClick.feedback_audio = audio;

        hoverClick.confirmer = true;
    }
}