using LabFusion.Data;
using LabFusion.Preferences.Client;
using LabFusion.Voice;

using UnityEngine;

using BoneLib.BoneMenu;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static readonly FusionDictionary<string, BoolElement> _tagElements = new();

    // Settings menu
    private static Page _serverSettingsCategory;
    private static Page _clientSettingsCategory;

    public static void CreateSettingsMenu(Page page)
    {
        // Root settings
        var settings = page.CreatePage("Settings", Color.gray);

        // Client settings
        _clientSettingsCategory = settings.CreatePage("Client Settings", Color.white);
        CreateClientSettingsMenu(_clientSettingsCategory);
    }

    private static void CreateClientSettingsMenu(Page page)
    {
        // Voice chat
        var voiceChatSubPanel = page.CreatePage("Voice Chat Settings", Color.white, 0, false);
        var voiceChatLink = page.CreatePageLink(voiceChatSubPanel);

        if (VoiceInfo.CanTalk)
        {
            CreateInputDevicesPage(voiceChatSubPanel);
        }

        if (VoiceInfo.CanHear)
        {
            CreateFloatPreference(voiceChatSubPanel, "Global Volume", 0.1f, 0f, 10f, ClientSettings.VoiceChat.GlobalVolume);
        }

        RemoveEmptyPage(page, voiceChatSubPanel, voiceChatLink);
    }

    private static void CreateInputDevicesPage(Page page)
    {
        var devices = VoiceInfo.InputDevices;

        var inputPreference = ClientSettings.VoiceChat.InputDevice;

        var inputPage = page.CreatePage("Input Devices", Color.cyan);

        Dictionary<string, FunctionElement> deviceElements = new();

        var defaultButton = inputPage.CreateFunction("Default", string.IsNullOrEmpty(inputPreference.Value) ? Color.green : Color.gray, () =>
        {
            inputPreference.Value = string.Empty;
        });

        deviceElements.Add(string.Empty, defaultButton);

        foreach (var device in devices)
        {
            var color = inputPreference.Value == device ? Color.green : Color.gray;

            var deviceButton = inputPage.CreateFunction(device, color, () =>
            {
                inputPreference.Value = device;
            });

            deviceElements.Add(device, deviceButton);
        }

        inputPreference.OnValueChanged += (value) =>
        {
            foreach (var element in deviceElements)
            {
                if (string.IsNullOrWhiteSpace(value) && string.IsNullOrEmpty(element.Key))
                {
                    element.Value.ElementColor = Color.green;
                    continue;
                }

                if (value == element.Key)
                {
                    element.Value.ElementColor = Color.green;
                    continue;
                }

                element.Value.ElementColor = Color.gray;
            }
        };
    }

}