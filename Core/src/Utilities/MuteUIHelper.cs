using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;

using SLZ.Rig;
using SLZ.UI;

using System;

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LabFusion.Utilities
{
    public static class MuteUIHelper {
        private static PageItem _mutePage = null;
        private static GameObject _muteIcon = null;

        public static void OnInitializeMelon() {
            FusionPreferences.ClientSettings.Muted.OnValueChanged += OnMutedChanged;
            FusionPreferences.ClientSettings.MutedIndicator.OnValueChanged += OnIndicatorChanged;
        }

        public static void OnDeinitializeMelon() {
            FusionPreferences.ClientSettings.Muted.OnValueChanged -= OnMutedChanged;
            FusionPreferences.ClientSettings.MutedIndicator.OnValueChanged -= OnIndicatorChanged;
        }

        private static void OnMutedChanged(bool value) {
            if (_mutePage != null) {
                _mutePage.name = value ? "Quick Unmute" : "Quick Mute";
            }

            UpdateMuteIcon();
        }

        private static void OnIndicatorChanged(bool value) {
            UpdateMuteIcon();
        }

        private static void UpdateMuteIcon() {
            if (_muteIcon != null) {
                _muteIcon.SetActive(VoiceHelper.ShowIndicator);
            }
        }

        public static void OnCreateMuteUI(RigManager manager)
        {
            // If this current networking layer does not support VC, don't bother showing these icons
            if (VoiceHelper.CanTalk)
            {
                // Insert quick mute button
                var popUpMenu = manager.uiRig.popUpMenu;
                var homePage = popUpMenu.radialPageView.m_HomePage;
                var mutedPref = FusionPreferences.ClientSettings.Muted;
                bool isMuted = mutedPref.GetValue();

                string name = isMuted ? "Quick Unmute" : "Quick Mute";

                _mutePage = new PageItem(name, PageItem.Directions.SOUTHEAST, (Action)(() =>
                {
                    mutedPref.SetValue(!mutedPref.GetValue());
                    popUpMenu.Deactivate();

                    FusionNotifier.Send(new FusionNotification()
                    {
                        isPopup = true,
                        isMenuItem = false,
                        message = mutedPref.GetValue() ? "Muted" : "Unmuted",
                    });
                }));

                homePage.items.Add(_mutePage);

                // Add mute icon
                _muteIcon = GameObject.Instantiate(FusionContentLoader.MutePopupPrefab, Vector3Extensions.up * -500f, QuaternionExtensions.identity);
                _muteIcon.name = "Mute Icon [FUSION]";
                var muteCamera = _muteIcon.GetComponent<Camera>();
                var cameraData = manager.openControllerRig.m_head.GetComponent<UniversalAdditionalCameraData>();
                cameraData.cameraStack.Add(muteCamera);

                _muteIcon.SetActive(VoiceHelper.ShowIndicator);
            }
        }

        public static void OnDestroyMuteUI(RigManager manager)
        {
            if (_mutePage != null)
            {
                var popUpMenu = manager.uiRig.popUpMenu;
                var homePage = popUpMenu.radialPageView.m_HomePage;
                homePage.items.RemoveAll((Il2CppSystem.Predicate<PageItem>)((i) => i.name == _mutePage.name));
                popUpMenu.radialPageView.Render(homePage);

                _mutePage = null;
            }

            if (!_muteIcon.IsNOC())
            {
                GameObject.Destroy(_muteIcon);
                _muteIcon = null;
            }
        }
    }
}
