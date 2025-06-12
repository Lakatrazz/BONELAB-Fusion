using LabFusion.UI.Popups;
using LabFusion.Preferences.Client;
using LabFusion.Voice;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;

using UnityEngine;
using UnityEngine.Rendering;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Utilities;

public static class MuteUIHelper
{
    private static PageItem _mutePage = null;

    private static Poolee _indicatorPoolee = null;
    private static GameObject _indicatorGameObject = null;
    private static Renderer _indicatorRenderer = null;

    private static Camera _headsetCamera = null;

    public const string NotificationTag = "Mute";

    public static void OnInitializeMelon()
    {
        ClientSettings.VoiceChat.Muted.OnValueChanged += OnMutedChanged;
        ClientSettings.VoiceChat.MutedIndicator.OnValueChanged += OnIndicatorChanged;

        RenderPipelineManager.beginCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
    }

    public static void OnDeinitializeMelon()
    {
        ClientSettings.VoiceChat.Muted.OnValueChanged -= OnMutedChanged;
        ClientSettings.VoiceChat.MutedIndicator.OnValueChanged -= OnIndicatorChanged;

        RenderPipelineManager.beginCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= (Il2CppSystem.Action<ScriptableRenderContext, Camera>)OnEndCameraRendering;
    }

    private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != _headsetCamera)
        {
            return;
        }

        if (_indicatorRenderer == null)
        {
            return;
        }

        _indicatorRenderer.enabled = true;
    }

    private static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != _headsetCamera)
        {
            return;
        }

        if (_indicatorRenderer == null)
        {
            return;
        }

        _indicatorRenderer.enabled = false;
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
        if (_indicatorGameObject == null)
        {
            return;
        }

        _indicatorGameObject.SetActive(VoiceInfo.ShowMuteIndicator);
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
        var mutedPref = ClientSettings.VoiceChat.Muted;
        bool isMuted = mutedPref.Value;

        string name = isMuted ? "Quick Unmute" : "Quick Mute";

        _mutePage = new PageItem(name, PageItem.Directions.SOUTHEAST, (Action)(() =>
        {
            mutedPref.Value = !mutedPref.Value;
            popUpMenu.Deactivate();

            Notifier.Cancel(NotificationTag);

            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                SaveToMenu = false,
                Message = mutedPref.Value ? "Muted" : "Unmuted",
                Tag = NotificationTag,
            });
        }));

        homePage.items.Add(_mutePage);

        // Add mute icon
        var openControllerRig = manager.ControllerRig.TryCast<OpenControllerRig>();
        Transform headset = openControllerRig.headset;

        var muteSpawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.MuteIndicatorReference);

        LocalAssetSpawner.Register(muteSpawnable);

        LocalAssetSpawner.Spawn(muteSpawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            // Store references to the spawned mute icon
            _indicatorPoolee = poolee;

            _indicatorGameObject = poolee.gameObject;
            _indicatorGameObject.SetActive(false);

            _indicatorGameObject.name = "Mute Icon [FUSION]";

            _indicatorRenderer = _indicatorGameObject.GetComponentInChildren<Renderer>();
            _indicatorRenderer.enabled = false;

            if (headset == null)
            {
                return;
            }

            // Store the headset camera
            _headsetCamera = headset.GetComponent<Camera>();

            // Parent the mute icon to the headset
            var iconTransform = _indicatorGameObject.transform;
            iconTransform.parent = headset;
            iconTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Update the visibility of the indicator
            UpdateMuteIcon();
        });
    }

    public static void OnDestroyMuteUI()
    {
        if (_mutePage != null)
        {
            var popUpMenu = UIRig.Instance.popUpMenu;
            var homePage = popUpMenu.radialPageView.m_HomePage;
            homePage.items.RemoveAll((Il2CppSystem.Predicate<PageItem>)((i) => i.name == _mutePage.name));
            popUpMenu.radialPageView.Render(homePage);

            _mutePage = null;
        }

        if (_indicatorPoolee != null)
        {
            _indicatorPoolee.Despawn();

            _indicatorPoolee = null;
            _indicatorGameObject = null;
            _indicatorRenderer = null;
        }
    }
}