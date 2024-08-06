using LabFusion.Extensions;
using LabFusion.Preferences;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using LabFusion.Voice;

namespace LabFusion.Utilities;

public static class MuteUIHelper
{
    private static PageItem _mutePage = null;
    private static GameObject _muteIcon = null;
    private static Renderer _muteRenderer = null;
    private static Camera _muteCamera = null;

    public static void OnInitializeMelon()
    {
        ClientSettings.Muted.OnValueChanged += OnMutedChanged;
        ClientSettings.MutedIndicator.OnValueChanged += OnIndicatorChanged;

        RenderPipelineManager.beginCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
    }

    public static void OnDeinitializeMelon()
    {
        ClientSettings.Muted.OnValueChanged -= OnMutedChanged;
        ClientSettings.MutedIndicator.OnValueChanged -= OnIndicatorChanged;

        RenderPipelineManager.beginCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
    }

    private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == _muteCamera && _muteRenderer != null)
        {
            _muteRenderer.enabled = true;
        }
    }

    private static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == _muteCamera && _muteRenderer != null)
        {
            _muteRenderer.enabled = false;
        }
    }

    private static void OnMutedChanged(bool value)
    {
        if (_mutePage != null)
        {
            _mutePage.name = value ? "Quick Unmute" : "Quick Mute";
        }

        UpdateMuteIcon();
    }

    private static void OnIndicatorChanged(bool value)
    {
        UpdateMuteIcon();
    }

    private static void UpdateMuteIcon()
    {
        if (_muteIcon != null)
        {
            _muteIcon.SetActive(VoiceInfo.ShowMuteIndicator);
        }
    }

    public static void OnCreateMuteUI(RigManager manager)
    {
        // If this current networking layer does not support VC, don't bother showing these icons
        if (!VoiceInfo.CanTalk)
        {
            return;
        }

        // Insert quick mute button
        var popUpMenu = UIRig.Instance.popUpMenu;
        var homePage = popUpMenu.radialPageView.m_HomePage;
        var mutedPref = ClientSettings.Muted;
        bool isMuted = mutedPref.Value;

        string name = isMuted ? "Quick Unmute" : "Quick Mute";

        _mutePage = new PageItem(name, PageItem.Directions.SOUTHEAST, (Action)(() =>
        {
            mutedPref.Value = !mutedPref.Value;
            popUpMenu.Deactivate();

            FusionNotifier.Send(new FusionNotification()
            {
                isPopup = true,
                isMenuItem = false,
                message = mutedPref.Value ? "Muted" : "Unmuted",
            });
        }));

        homePage.items.Add(_mutePage);

        // Add mute icon
        var openControllerRig = manager.ControllerRig.TryCast<OpenControllerRig>();
        Transform headset = openControllerRig.headset;
        FusionContentLoader.MutePopupPrefab.Load((go) =>
        {
            if (headset == null)
            {
                return;
            }

            _muteIcon = GameObject.Instantiate(go, headset);
            _muteIcon.SetActive(false);

            _muteIcon.name = "Mute Icon [FUSION]";

            _muteRenderer = _muteIcon.GetComponentInChildren<Renderer>();
            _muteRenderer.enabled = false;
            _muteCamera = _muteIcon.GetComponent<Camera>();

            // Add mute camera to the camera stack
            var cameraData = headset.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(_muteCamera);

            // Configure the mute camera's data so that it only renders what it needs to
            var muteCameraData = _muteIcon.GetComponent<UniversalAdditionalCameraData>();

            // Make sure the volume layer mask is the same as the normal camera
            muteCameraData.volumeLayerMask = cameraData.volumeLayerMask;

            muteCameraData.renderPostProcessing = false;
            muteCameraData.m_EnableVolumetrics = false;
            muteCameraData.renderShadows = false;
            muteCameraData.requiresColorOption = CameraOverrideOption.Off;
            muteCameraData.requiresColorTexture = false;
            muteCameraData.requiresDepthOption = CameraOverrideOption.Off;
            muteCameraData.requiresDepthTexture = false;

            _muteIcon.SetActive(VoiceInfo.ShowMuteIndicator);
        });
    }

    public static void OnDestroyMuteUI(RigManager manager)
    {
        if (_mutePage != null)
        {
            var popUpMenu = UIRig.Instance.popUpMenu;
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