using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;

using SLZ.Rig;
using SLZ.UI;

using System;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LabFusion.Utilities
{
    public static class MuteUIHelper {
        private static PageItem _mutePage = null;
        private static GameObject _muteIcon = null;
        private static Renderer _muteRenderer = null;
        private static Camera _muteCamera = null;

        public static void OnInitializeMelon() {
            FusionPreferences.ClientSettings.Muted.OnValueChanged += OnMutedChanged;
            FusionPreferences.ClientSettings.MutedIndicator.OnValueChanged += OnIndicatorChanged;

            RenderPipelineManager.beginCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
        }

        public static void OnDeinitializeMelon() {
            FusionPreferences.ClientSettings.Muted.OnValueChanged -= OnMutedChanged;
            FusionPreferences.ClientSettings.MutedIndicator.OnValueChanged -= OnIndicatorChanged;

            RenderPipelineManager.beginCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
        }

        private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == _muteCamera && _muteRenderer != null)
                _muteRenderer.enabled = true;
        }

        private static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == _muteCamera && _muteRenderer != null)
                _muteRenderer.enabled = false;
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
                Transform playerHead = manager.openControllerRig.m_head;
                _muteIcon = GameObject.Instantiate(FusionContentLoader.MutePopupPrefab, playerHead);
                _muteIcon.name = "Mute Icon [FUSION]";
                _muteRenderer = _muteIcon.GetComponentInChildren<Renderer>();
                _muteRenderer.enabled = false;
                _muteCamera = _muteIcon.GetComponent<Camera>();
                var cameraData = playerHead.GetComponent<UniversalAdditionalCameraData>();
                cameraData.cameraStack.Add(_muteCamera);

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
