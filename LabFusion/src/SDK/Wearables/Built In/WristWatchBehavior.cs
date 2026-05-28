using Il2CppSLZ.Marrow;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Math;
using LabFusion.Player;

using LabFusion.UI;
using LabFusion.UI.Elements;
using LabFusion.UI.Resources;
using LabFusion.UI.Styles;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WristWatchBehavior : IWearableComponent
{
    public enum PanelState
    {
        Closed,

        Projecting,

        Opened,

        Aligning,

        Retracting,
    }

    public static AudioPlayerSettings PanelAudioPlayerSettings => new()
    {
        Mixer = LocalAudioPlayer.HardInteraction,
        Volume = 0.2f,
    };

    public const float MinPanelSize = 0.05f;

    public const float MaxPanelSize = 0.2f;

    public const float PanelScreenSize = 0.5f;

    public const float PanelNeutralSize = 0.1f;

    public const float PanelDistance = 0.07f;

    public const float EyeBarrierDistance = 0.05f;

    public const float SmoothDecay = 12f;

    public const float ObserveLookTime = 0.3f;

    public const int Resolution = 200;

    public bool IsLocal { get; set; } = false;

    public Transform MainInstanceTransform { get; set; } = null;

    public Transform Origin { get; set; } = null;
    public Transform Panel { get; set; } = null;
    public Transform Surface { get; set; } = null;

    public RectTransform Canvas { get; set; } = null;

    public Transform Head { get; set; } = null;

    public RigManager RigManager { get; set; } = null;

    public Transform ControllerRigTransform { get; set; } = null;

    public Transform Effects { get; set; } = null;

    public Transform UI { get; set; } = null;

    public List<LineRenderer> Lines { get; } = new();

    public List<Transform> Corners { get; } = new();

    public List<Transform> Flares { get; } = new();

    public UIElementView RootView { get; private set; } = null;

    private bool _isOpen = false;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value)
            {
                return;
            }

            _isOpen = value;
        }
    }

    public PanelState State { get; private set; } = PanelState.Closed;

    public float StateElapsed { get; set; } = 0f;

    private Vector3 _lastPanelPositionInRig = Vector3.zero;
    private Quaternion _lastPanelRotationInRig = Quaternion.identity;
    private float _lastPanelScaleFactor = 0f;

    private Vector3 _stateStartPositionInRig = Vector3.zero;
    private Quaternion _stateStartRotationInRig = Quaternion.identity;
    private float _stateStartScaleFactor = 0f;

    private float _observedElapsed = 0f;

    public void OnInitialize(bool local, PlayerID playerID = null)
    {
        IsLocal = local;
    }

    public void OnMainInstanceCreated(GameObject mainInstance, RigManager rigManager)
    {
        _isOpen = false;

        RigManager = rigManager;

        var transform = mainInstance.transform;

        MainInstanceTransform = transform;

        UI = transform.Find("UI");

        Origin = UI.Find("Origin");
        Panel = Origin.Find("Panel");
        Surface = Origin.Find("Surface");

        Canvas = Panel.Find("Canvas").GetComponent<RectTransform>();

        RootView = Canvas.Find("view_UIElement").GetComponent<UIElementView>();

        Head = rigManager.ControllerRig.TryCast<OpenControllerRig>().headset;
        ControllerRigTransform = rigManager.ControllerRig.transform;

        Lines.Clear();
        Corners.Clear();
        Flares.Clear();

        Effects = transform.Find("Effects");

        var effectsOrigin = Effects.Find("Origin");

        var linesRoot = effectsOrigin.Find("Lines");
        var cornersRoot = Panel.Find("Corners");
        var flaresRoot = effectsOrigin.Find("Flares");

        for (var i = 0; i < linesRoot.childCount; i++)
        {
            Lines.Add(linesRoot.GetChild(i).GetComponent<LineRenderer>());
        }

        for (var i = 0; i < cornersRoot.childCount; i++)
        {
            Corners.Add(cornersRoot.GetChild(i));
        }

        for (var i = 0; i < flaresRoot.childCount; i++)
        {
            Flares.Add(flaresRoot.GetChild(i));
        }

        ShowPanel(false);
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

        CheckPanelObservation(deltaTime);
        SolveState(deltaTime);

        if (State != PanelState.Closed)
        {
            DrawLineRenderers();
            DrawFlares();
        }
    }

    private void ShowPanel(bool show)
    {
        Effects.gameObject.SetActive(show);
        UI.gameObject.SetActive(show);

        if (show)
        {
            ApplyCanvasResolution();
            DrawUITree();
        }
    }

    private void ApplyCanvasResolution()
    {
        float scale = 1f / Resolution;
        Canvas.localScale = Vector3Extensions.One * scale;
        Canvas.sizeDelta = Vector2.one * Resolution;
    }

    private void DrawUITree()
    {
        var root = new UIElement();

        root.Style.BackgroundColor = new Color(0f, 0f, 0f, 0.3f);
        root.Style.Padding = new BorderOffsets(5, 5, 5, 5);

        // Top info group
        var topGroup = new UIElement();
        topGroup.Style.AlignItems = Align.Center;
        topGroup.Style.Margins = new BorderOffsets(0, 0, 5, 5);

        var gamemodeTitleLabel = new LabelElement("00:00:00");

        gamemodeTitleLabel.Style.Font = UIResources.GetCommonFont(CommonFonts.BalooBhai2_SemiBold);
        gamemodeTitleLabel.Style.FontSize = 30f;

        topGroup.Add(gamemodeTitleLabel);

        root.Add(topGroup);

        var horizontalLine = new UIElement();
        horizontalLine.Style.BackgroundColor = Color.white;
        horizontalLine.Style.Height = 2.5f;
        root.Add(horizontalLine);

        // Center info group
        var centerGroup = new UIElement();
        centerGroup.Style.FlexGrow = 1f;
        centerGroup.Style.Direction = Direction.Row;
        centerGroup.Style.JustifyContent = Justify.Center;
        centerGroup.Style.AlignItems = Align.Center;
        centerGroup.Style.Padding = new BorderOffsets(5, 5, 10, 10);

        var stockGroup = new UIElement();
        stockGroup.Style.Direction = Direction.Row;
        stockGroup.Style.JustifyContent = Justify.Start;
        stockGroup.Style.AlignItems = Align.Center;
        stockGroup.Style.Margins = new BorderOffsets(10, 10, 5, 5);

        var stockIcon = new UIElement();
        stockIcon.Style.Width = 50f;
        stockIcon.Style.Height = 50f;
        stockIcon.Style.BackgroundImage = UIResources.GetCommonIcon(CommonIcons.SkullRetro);
        stockGroup.Add(stockIcon);

        var stockLabel = new LabelElement("x3");
        stockLabel.Style.Margins = new BorderOffsets(5, 0, 0, 0);
        stockLabel.Style.Font = UIResources.GetCommonFont(CommonFonts.BalooBhai2_SemiBold);
        stockLabel.Style.FontSize = 20f;
        stockGroup.Add(stockLabel);

        var percentGroup = new UIElement();
        percentGroup.Style.Direction = Direction.Row;
        percentGroup.Style.JustifyContent = Justify.Center;
        percentGroup.Style.AlignItems = Align.Center;
        percentGroup.Style.Margins = new BorderOffsets(10, 10, 5, 5);

        var percentLabel = new LabelElement("43%");
        percentLabel.Style.Font = UIResources.GetCommonFont(CommonFonts.BalooBhai2_SemiBold);
        percentLabel.Style.FontSize = 20f;
        percentGroup.Add(percentLabel);

        centerGroup.Add(stockGroup);

        var verticalLine = new UIElement();
        verticalLine.Style.BackgroundColor = Color.white;
        verticalLine.Style.Width = 2.5f;
        verticalLine.Style.AlignSelfStretch = true;
        centerGroup.Add(verticalLine);

        centerGroup.Add(percentGroup);

        root.Add(centerGroup);

        UIElementDrawer.DrawUITree(root, RootView);
    }

    private void CheckPanelObservation(float deltaTime)
    {
        float heightScale = GetHeightScale();

        var originPosition = Origin.position;
        var eyePosition = Head.position;

        var eyeToOriginVector = originPosition - eyePosition;

        float eyeToOriginDistance = eyeToOriginVector.magnitude;

        var lookDirection = Head.forward;
        var eyeToOriginDirection = eyeToOriginVector.normalized;
        var panelDirection = -Origin.up;

        float angleMultiplier = IsOpen ? 0.8f : 1f;
        float maxDistance = IsOpen ? 0.3f : 0.2f;

        bool lookingAtOrigin = Vector3.Dot(lookDirection, eyeToOriginDirection) >= 0.95f * angleMultiplier;
        bool originOutwards = Vector3.Dot(panelDirection, eyeToOriginDirection) >= 0.6f * angleMultiplier;
        bool originClose = (eyeToOriginDistance / heightScale) <= maxDistance;

        bool observed = lookingAtOrigin && originOutwards && originClose;

        if (observed)
        {
            _observedElapsed += deltaTime;
        }
        else
        {
            _observedElapsed = 0f;
        }

        IsOpen = observed && _observedElapsed >= ObserveLookTime;
    }

    private void SolveState(float deltaTime)
    {
        float heightScale = GetHeightScale();

        StateElapsed += deltaTime;

        switch (State)
        {
            default:
            case PanelState.Closed:
                if (IsOpen)
                {
                    SwitchState(PanelState.Projecting);

                    Panel.SetPositionAndRotation(Origin.position, Origin.rotation);
                    Panel.localScale = Vector3.zero;

                    StorePanelState(Panel.position, Panel.rotation, 0f);
                    ShowPanel(true);

                    LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.JinglePositiveHolographic00Reference), Origin.position, PanelAudioPlayerSettings);
                }
                break;
            case PanelState.Projecting:
                var projectingProgress = StateElapsed / 0.2f;

                SolveProjectingState(Smoothing.EaseOutCubic(projectingProgress), heightScale);

                if (projectingProgress >= 1f)
                {
                    SwitchState(PanelState.Opened);
                }
                break;
            case PanelState.Opened:
                if (!IsOpen)
                {
                    SwitchState(PanelState.Aligning);

                    LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.JingleNegativeHolographic00Reference), Origin.position, PanelAudioPlayerSettings);
                    break;
                }

                SolvePanelActiveTransform(deltaTime);
                break;
            case PanelState.Aligning:
                float aligningProgress = StateElapsed / 0.25f;

                SolveAlignState(Smoothing.EaseInCubic(aligningProgress), heightScale);

                if (aligningProgress >= 1f)
                {
                    SwitchState(PanelState.Retracting);
                }
                break;
            case PanelState.Retracting:
                float retractingProgress = StateElapsed / 0.2f;

                SolveProjectingState(Smoothing.EaseOutCubic(1f - retractingProgress), heightScale);

                if (retractingProgress >= 1f)
                {
                    SwitchState(PanelState.Closed);
                    ShowPanel(false);
                }
                break;
        }
    }

    private void SolveProjectingState(float projection, float heightScale)
    {
        var startPosition = Origin.position;

        var endPosition = SolvePanelNeutralPosition(heightScale);
        var endRotation = SolvePanelNeutralRotation();

        var newPosition = Vector3.Lerp(startPosition, endPosition, projection);

        Panel.SetPositionAndRotation(newPosition, endRotation);
        Panel.localScale = Vector3.zero;

        StorePanelState(newPosition, endRotation, 0f);
    }

    private void SolveAlignState(float progress, float heightScale)
    {
        var startPosition = ControllerRigTransform.TransformPoint(_stateStartPositionInRig);
        var startRotation = ControllerRigTransform.rotation * _stateStartRotationInRig;
        var startScaleFactor = _stateStartScaleFactor;

        var endPosition = SolvePanelNeutralPosition(heightScale);
        var endRotation = SolvePanelNeutralRotation();
        var endScaleFactor = 0f;

        var newPosition = Vector3.Lerp(startPosition, endPosition, progress);
        var newRotation = Quaternion.Slerp(startRotation, endRotation, progress);

        var newScaleFactor = Mathf.Lerp(startScaleFactor, endScaleFactor, progress);
        var newScale = GetPanelHierarchyInverseScale() * newScaleFactor;

        Panel.SetPositionAndRotation(newPosition, newRotation);
        Panel.localScale = newScale;

        StorePanelState(newPosition, newRotation, newScaleFactor);
    }

    private void SwitchState(PanelState state)
    {
        State = state;
        StateElapsed = 0f;

        StorePanelState(Panel.position, Panel.rotation, _lastPanelScaleFactor);

        _stateStartPositionInRig = _lastPanelPositionInRig;
        _stateStartRotationInRig = _lastPanelRotationInRig;
        _stateStartScaleFactor = _lastPanelScaleFactor;
    }

    private Vector3 SolvePanelNeutralPosition(float heightScale) => Origin.position + PanelDistance * heightScale * Origin.up;

    public Quaternion SolvePanelNeutralRotation() => Origin.rotation * Quaternion.AngleAxis(90f, Vector3.right);

    public static float SolvePanelNeutralScaleFactor(float heightScale) => PanelNeutralSize * heightScale;

    private void SolvePanelActiveTransform(float deltaTime)
    {
        float heightScale = GetHeightScale();

        var originPosition = Origin.position;
        var eyePosition = Head.position;

        var eyeToOriginVector = originPosition - eyePosition;

        var lastPanelPosition = ControllerRigTransform.TransformPoint(_lastPanelPositionInRig);
        var lastPanelRotation = ControllerRigTransform.rotation * _lastPanelRotationInRig;

        float decay = Smoothing.CalculateDecay(SmoothDecay, deltaTime);

        var smoothPosition = Vector3.Lerp(lastPanelPosition, SolvePanelActivePosition(eyeToOriginVector, heightScale), decay);

        var panelPosition = LimitPositionToSurface(smoothPosition);
        var panelRotation = Quaternion.Slerp(lastPanelRotation, SolvePanelActiveRotation(eyeToOriginVector), decay);
        var panelScaleFactor = Mathf.Lerp(_lastPanelScaleFactor, SolvePanelActiveScaleFactor(panelPosition, eyePosition, heightScale), decay);

        var panelScale = GetPanelHierarchyInverseScale() * panelScaleFactor;

        Panel.SetPositionAndRotation(panelPosition, panelRotation);
        Panel.localScale = panelScale;

        StorePanelState(panelPosition, panelRotation, panelScaleFactor);
    }

    private Vector3 SolvePanelActivePosition(Vector3 eyeToOriginVector, float heightScale)
    {
        var originPosition = Origin.position;

        float distance = eyeToOriginVector.magnitude;
        var lookDirection = eyeToOriginVector.normalized;

        float neutralDistance = PanelDistance * heightScale;
        float surfaceDistance = (Surface.position - originPosition).magnitude;

        float offsetDistance = EyeBarrierDistance * heightScale;

        float panelDistance = MathF.Max(surfaceDistance, MathF.Min(neutralDistance, distance - offsetDistance));

        var panelPosition = originPosition - panelDistance * lookDirection;

        return panelPosition;
    }

    private Quaternion SolvePanelActiveRotation(Vector3 eyeToOriginVector)
    {
        var up = ControllerRigTransform.up;

        var panelRotation = Quaternion.LookRotation(eyeToOriginVector.normalized, up);

        return panelRotation;
    }

    private static float SolvePanelActiveScaleFactor(Vector3 panelPosition, Vector3 eyePosition, float heightScale)
    {
        float distance = (panelPosition - eyePosition).magnitude;

        float panelScale = Mathf.Clamp(PanelScreenSize * distance, MinPanelSize * heightScale, MaxPanelSize * heightScale);

        return panelScale;
    }

    private Vector3 GetPanelHierarchyInverseScale()
    {
        var rootScale = MainInstanceTransform.localScale;
        var inverseScale = new Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z);
        return inverseScale;
    }

    private Vector3 LimitPositionToSurface(Vector3 position)
    {
        var positionInSurface = Surface.InverseTransformPoint(position);
        positionInSurface.y = MathF.Max(0f, positionInSurface.y);
        position = Surface.TransformPoint(positionInSurface);

        return position;
    }

    private float GetHeightScale() => RigManager.avatar.height / MarrowConstants.StandardHeight;

    private void StorePanelState(Vector3 panelPosition, Quaternion panelRotation, float panelScaleFactor)
    {
        _lastPanelPositionInRig = ControllerRigTransform.InverseTransformPoint(panelPosition);
        _lastPanelRotationInRig = Quaternion.Inverse(ControllerRigTransform.rotation) * panelRotation;
        _lastPanelScaleFactor = panelScaleFactor;
    }

    private void DrawLineRenderers()
    {
        for (var i = 0; i < Lines.Count; i++)
        {
            var line = Lines[i];
            var corner = Corners[i];

            line.SetPosition(1, line.transform.InverseTransformPoint(corner.position));
        }
    }

    private void DrawFlares()
    {
        for (var i = 0; i < Flares.Count; i++)
        {
            var flare = Flares[i];
            var corner = Corners[i];

            flare.position = corner.position;
        }
    }

    public void OnDeinitialize()
    {
    }
}
