using LabFusion.Marrow;
using LabFusion.Math;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WristWatchPanel
{
    public enum PanelState
    {
        Closed,

        Projecting,

        Opened,

        Aligning,

        Retracting,
    }

    public const float MinPanelSize = 0.05f;

    public const float MaxPanelSize = 0.2f;

    public const float PanelScreenSize = 0.5f;

    public const float PanelNeutralSize = 0.1f;

    public const float PanelDistance = 0.07f;

    public const float EyeBarrierDistance = 0.05f;

    public const float SmoothDecay = 12f;

    public const float ObserveLookTime = 0.3f;

    public WristWatchReferences References { get; set; } = null;

    public PanelState State { get; private set; } = PanelState.Closed;

    public float StateElapsed { get; set; } = 0f;

    public bool IsOpen { get; set; } = false;

    public bool HasShown { get; set; } = false;

    public bool ForceHide { get; set; } = false;

    public event Action Shown;

    private Vector3 _lastPanelPositionInRig = Vector3.zero;
    private Quaternion _lastPanelRotationInRig = Quaternion.identity;
    private float _lastPanelScaleFactor = 0f;

    private Vector3 _stateStartPositionInRig = Vector3.zero;
    private Quaternion _stateStartRotationInRig = Quaternion.identity;
    private float _stateStartScaleFactor = 0f;

    private float _observedElapsed = 0f;

    public void Reinitialize(WristWatchReferences references)
    {
        IsOpen = false;

        References = references;

        ShowPanel(false);
    }

    public void Tick(float deltaTime)
    {
        CheckPanelObservation(deltaTime);

        SolveState(deltaTime);
    }

    private void ShowPanel(bool show)
    {
        References.Effects.gameObject.SetActive(show);
        References.UI.gameObject.SetActive(show);

        if (show)
        {
            Shown?.Invoke();

            HasShown = true;
        }
    }


    private void CheckPanelObservation(float deltaTime)
    {
        if (ForceHide)
        {
            IsOpen = false;
            _observedElapsed = 0f;
            return;
        }

        float heightScale = GetHeightScale();

        var originPosition = References.Origin.position;
        var eyePosition = References.Head.position;

        var eyeToOriginVector = originPosition - eyePosition;

        float eyeToOriginDistance = eyeToOriginVector.magnitude;

        var lookDirection = References.Head.forward;
        var eyeToOriginDirection = eyeToOriginVector.normalized;
        var panelDirection = -References.Origin.up;

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

                    References.Panel.SetPositionAndRotation(References.Origin.position, References.Origin.rotation);
                    References.Panel.localScale = Vector3.zero;

                    StorePanelState(References.Panel.position, References.Panel.rotation, 0f);
                    ShowPanel(true);

                    LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.JinglePositiveHolographic00Reference), References.Origin.position, WristWatchReferences.WatchAudioPlayerSettings);
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

                    LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.JingleNegativeHolographic00Reference), References.Origin.position, WristWatchReferences.WatchAudioPlayerSettings);
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
        var startPosition = References.Origin.position;

        var endPosition = SolvePanelNeutralPosition(heightScale);
        var endRotation = SolvePanelNeutralRotation();

        var newPosition = Vector3.Lerp(startPosition, endPosition, projection);

        References.Panel.SetPositionAndRotation(newPosition, endRotation);
        References.Panel.localScale = Vector3.zero;

        StorePanelState(newPosition, endRotation, 0f);
    }

    private void SolveAlignState(float progress, float heightScale)
    {
        var startPosition = References.ControllerRigTransform.TransformPoint(_stateStartPositionInRig);
        var startRotation = References.ControllerRigTransform.rotation * _stateStartRotationInRig;
        var startScaleFactor = _stateStartScaleFactor;

        var endPosition = SolvePanelNeutralPosition(heightScale);
        var endRotation = SolvePanelNeutralRotation();
        var endScaleFactor = 0f;

        var newPosition = Vector3.Lerp(startPosition, endPosition, progress);
        var newRotation = Quaternion.Slerp(startRotation, endRotation, progress);

        var newScaleFactor = Mathf.Lerp(startScaleFactor, endScaleFactor, progress);
        var newScale = GetPanelHierarchyInverseScale() * newScaleFactor;

        References.Panel.SetPositionAndRotation(newPosition, newRotation);
        References.Panel.localScale = newScale;

        StorePanelState(newPosition, newRotation, newScaleFactor);
    }

    private void SwitchState(PanelState state)
    {
        State = state;
        StateElapsed = 0f;

        StorePanelState(References.Panel.position, References.Panel.rotation, _lastPanelScaleFactor);

        _stateStartPositionInRig = _lastPanelPositionInRig;
        _stateStartRotationInRig = _lastPanelRotationInRig;
        _stateStartScaleFactor = _lastPanelScaleFactor;
    }

    private Vector3 SolvePanelNeutralPosition(float heightScale) => References.Origin.position + PanelDistance * heightScale * References.Origin.up;

    public Quaternion SolvePanelNeutralRotation() => References.Origin.rotation * Quaternion.AngleAxis(90f, Vector3.right);

    public static float SolvePanelNeutralScaleFactor(float heightScale) => PanelNeutralSize * heightScale;

    private void SolvePanelActiveTransform(float deltaTime)
    {
        float heightScale = GetHeightScale();

        var originPosition = References.Origin.position;
        var eyePosition = References.Head.position;

        var eyeToOriginVector = originPosition - eyePosition;

        var lastPanelPosition = References.ControllerRigTransform.TransformPoint(_lastPanelPositionInRig);
        var lastPanelRotation = References.ControllerRigTransform.rotation * _lastPanelRotationInRig;

        float decay = Smoothing.CalculateDecay(SmoothDecay, deltaTime);

        var smoothPosition = Vector3.Lerp(lastPanelPosition, SolvePanelActivePosition(eyeToOriginVector, heightScale), decay);

        var panelPosition = LimitPositionToSurface(smoothPosition);
        var panelRotation = Quaternion.Slerp(lastPanelRotation, SolvePanelActiveRotation(eyeToOriginVector), decay);
        var panelScaleFactor = Mathf.Lerp(_lastPanelScaleFactor, SolvePanelActiveScaleFactor(panelPosition, eyePosition, heightScale), decay);

        var panelScale = GetPanelHierarchyInverseScale() * panelScaleFactor;

        References.Panel.SetPositionAndRotation(panelPosition, panelRotation);
        References.Panel.localScale = panelScale;

        StorePanelState(panelPosition, panelRotation, panelScaleFactor);
    }

    private Vector3 SolvePanelActivePosition(Vector3 eyeToOriginVector, float heightScale)
    {
        var originPosition = References.Origin.position;

        float distance = eyeToOriginVector.magnitude;
        var lookDirection = eyeToOriginVector.normalized;

        float neutralDistance = PanelDistance * heightScale;
        float surfaceDistance = (References.Surface.position - originPosition).magnitude;

        float offsetDistance = EyeBarrierDistance * heightScale;

        float panelDistance = MathF.Max(surfaceDistance, MathF.Min(neutralDistance, distance - offsetDistance));

        var panelPosition = originPosition - panelDistance * lookDirection;

        return panelPosition;
    }

    private Quaternion SolvePanelActiveRotation(Vector3 eyeToOriginVector)
    {
        var up = References.ControllerRigTransform.up;

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
        var rootScale = References.Root.localScale;
        var inverseScale = new Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z);
        return inverseScale;
    }

    private Vector3 LimitPositionToSurface(Vector3 position)
    {
        var positionInSurface = References.Surface.InverseTransformPoint(position);
        positionInSurface.y = MathF.Max(0f, positionInSurface.y);
        position = References.Surface.TransformPoint(positionInSurface);

        return position;
    }

    private float GetHeightScale() => References.RigManager.avatar.height / MarrowConstants.StandardHeight;

    private void StorePanelState(Vector3 panelPosition, Quaternion panelRotation, float panelScaleFactor)
    {
        _lastPanelPositionInRig = References.ControllerRigTransform.InverseTransformPoint(panelPosition);
        _lastPanelRotationInRig = Quaternion.Inverse(References.ControllerRigTransform.rotation) * panelRotation;
        _lastPanelScaleFactor = panelScaleFactor;
    }
}
