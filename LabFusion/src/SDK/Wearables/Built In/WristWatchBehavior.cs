using Il2CppSLZ.Marrow;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Player;

using LabFusion.UI.Elements;
using LabFusion.UI.Resources;
using LabFusion.UI.Styles;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WristWatchBehavior : IWearableComponent
{
    public enum WatchState
    {
        Inactive = 0,

        Beeping = 1,

        Active = 2,
    }

    public const int Resolution = 200;

    public static readonly int StateHash = Animator.StringToHash("State");

    public bool IsLocal { get; set; } = false;

    public WristWatchReferences References { get; set; } = null;

    public WristWatchPanel Panel { get; } = new();

    public WatchState State { get; private set; } = WatchState.Inactive;

    public bool HasAvailableUI { get; set; } = false;
    public bool HasDrawnUI { get; set; } = false;

    public bool IsAwaitingBeep { get; set; } = false;

    private float _awaitingBeepElapsed = 0f;

    public void OnInitialize(bool local, PlayerID playerID = null)
    {
        Panel.Shown += OnPanelShown;

        IsLocal = local;

        if (IsLocal)
        {
            WristWatchManager.ActiveUIChanged += OnActiveUIChanged;
            WristWatchManager.WatchBeeped += OnWatchBeeped;
        }
    }

    public void OnDeinitialize()
    {
        if (IsLocal)
        {
            WristWatchManager.ActiveUIChanged -= OnActiveUIChanged;
        }
    }

    public void OnMainInstanceCreated(GameObject mainInstance, RigManager rigManager)
    {
        if (!IsLocal)
        {
            return;
        }

        References = new WristWatchReferences();
        References.GetReferences(mainInstance.transform, rigManager);

        Panel.Reinitialize(References);
    }

    public void OnReflectionInstanceCreated(GameObject reflectionInstance)
    {
        // Disable all effects for the reflection
        var transform = reflectionInstance.transform;

        transform.Find("Effects").gameObject.SetActive(false);
        transform.Find("UI").gameObject.SetActive(false);
    }

    public void OnTick(float deltaTime)
    {
        if (!IsLocal)
        {
            return;
        }

        SolveState(deltaTime);

        Panel.ForceHide = !HasAvailableUI;
        Panel.Tick(deltaTime);

        if (Panel.State != WristWatchPanel.PanelState.Closed)
        {
            DrawLineRenderers();
            DrawFlares();
        }
    }

    private void SolveState(float deltaTime)
    {
        if (!HasAvailableUI)
        {
            if (State != WatchState.Inactive)
            {
                SwitchState(WatchState.Inactive);
            }
            return;
        }

        if (IsAwaitingBeep)
        {
            _awaitingBeepElapsed += deltaTime;

            if (_awaitingBeepElapsed > 0.25f)
            {
                SwitchState(WatchState.Beeping);
            }

            return;
        }

        switch (State)
        {
            default:
            case WatchState.Inactive:
                SwitchState(WatchState.Active);
                break;
            case WatchState.Beeping:
                if (Panel.HasShown)
                {
                    SwitchState(WatchState.Active);
                }
                break;
        }
    }

    private void OnPanelShown()
    {
        ApplyCanvasResolution();

        if (!HasDrawnUI)
        {
            DrawUITree();
        }
    }

    private void ApplyCanvasResolution()
    {
        float scale = 1f / Resolution;
        References.Canvas.localScale = Vector3Extensions.One * scale;
        References.Canvas.sizeDelta = Vector2.one * Resolution;
    }

    private void DrawUITree()
    {
        var activeUI = WristWatchManager.ActiveUI;

        if (activeUI == null)
        {
            References.RootView.UnassignElement();
            return;
        }

        var wearableUI = activeUI.CreateWearableUI();

        if (wearableUI == null)
        {
            References.RootView.UnassignElement();
            return;
        }

        var root = CreatePanelRoot();

        root.Add(wearableUI);

        root.SetExternalStyleSheets(new List<StyleSheet>()
        {
            CommonStyleSheets.DefaultStyleSheet,
            CommonStyleSheets.WatchStyleSheet,
        });

        root.ResolveStyle();

        References.RootView.AssignElement(root);

        HasDrawnUI = true;
    }

    private static UIElement CreatePanelRoot()
    {
        var root = new UIElement();

        root.Style.BackgroundColor = new Color(0f, 0f, 0f, 0.3f);
        root.Style.Padding = new BorderOffsets(5, 5, 5, 5);

        return root;
    }

    private void DrawLineRenderers()
    {
        for (var i = 0; i < References.Lines.Count; i++)
        {
            var line = References.Lines[i];
            var corner = References.Corners[i];

            line.SetPosition(1, line.transform.InverseTransformPoint(corner.position));
        }
    }

    private void DrawFlares()
    {
        for (var i = 0; i < References.Flares.Count; i++)
        {
            var flare = References.Flares[i];
            var corner = References.Corners[i];

            flare.position = corner.position;
        }
    }

    private void SwitchState(WatchState state)
    {
        State = state;

        var animator = References.Animator;

        animator.enabled = true;
        animator.SetInteger(StateHash, (int)State);

        switch (state)
        {
            case WatchState.Beeping:
                IsAwaitingBeep = false;
                Panel.HasShown = false;

                _awaitingBeepElapsed = 0f;

                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.JinglePositiveHolographic01Reference), References.Origin.position, WristWatchReferences.WatchAudioPlayerSettings);
                break;
            case WatchState.Active:
                Panel.HasShown = true;
                break;
        }
    }

    private void OnActiveUIChanged(IWearableUIProvider wearableUIProvider)
    {
        HasAvailableUI = wearableUIProvider != null;
        Panel.HasShown = false;
        HasDrawnUI = false;
    }

    private void OnWatchBeeped()
    {
        IsAwaitingBeep = true;
        Panel.HasShown = false;
        _awaitingBeepElapsed = 0f;
    }
}
