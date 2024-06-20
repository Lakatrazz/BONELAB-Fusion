using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Data;
using LabFusion.Extensions;

using MelonLoader;

using System.Collections;

namespace LabFusion.Entities;

public class RigGrabber
{
    public class GrabberData
    {
        public Grip grip;
        public SimpleTransform? targetInBase = null;
    }

    private RigReferenceCollection _references = null;

    private bool _isCulled = false;

    private FusionDictionary<Handedness, GrabberData> _lastGrabs = new();

    public RigGrabber(RigReferenceCollection references)
    {
        _references = references;
    }

    public void OnEntityCull(bool isInactive)
    {
        _isCulled = isInactive;

        if (isInactive)
        {
            DetachWithoutClear(Handedness.LEFT);
            DetachWithoutClear(Handedness.RIGHT);
        }
        else
        {
            MelonCoroutines.Start(WaitAndUncullGrips());
        }
    }

    private IEnumerator WaitAndUncullGrips()
    {
        for (var i = 0; i < 120; i++)
        {
            yield return null;
        }

        UncullGrip(Handedness.LEFT);
        UncullGrip(Handedness.RIGHT);
    }

    public void Attach(Handedness handedness, Grip grip, SimpleTransform? targetInBase = null)
    {
        _lastGrabs[handedness] = new GrabberData()
        {
            grip = grip,
            targetInBase = targetInBase
        };

        if (_isCulled)
        {
            return;
        }

        var hand = _references.GetHand(handedness);

        if (hand == null)
        {
            return;
        }

        if (grip)
        {
            // Detach existing grip
            hand.TryDetach();

            // Check if the grip can be interacted with
            if (grip.IsInteractionDisabled || (grip.HasHost && grip.Host.IsInteractionDisabled))
                return;

            // Attach the hand
            grip.TryAttach(hand, false, targetInBase);
        }
    }

    public void Detach(Handedness handedness)
    {
        _lastGrabs.Remove(handedness);

        DetachWithoutClear(handedness);
    }

    private void DetachWithoutClear(Handedness handedness)
    {
        var hand = _references.GetHand(handedness);

        if (hand == null)
        {
            return;
        }

        hand.TryDetach();
    }

    private void UncullGrip(Handedness handedness)
    {
        if (!_lastGrabs.TryGetValue(handedness, out var data))
        {
            return;
        }

        if (data.grip.IsNOC())
        {
            _lastGrabs.Remove(handedness);
            return;
        }

        Attach(handedness, data.grip, data.targetInBase);
    }
}