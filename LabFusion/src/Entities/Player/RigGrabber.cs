using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Extensions;
using LabFusion.Utilities;
using MelonLoader;

using System.Collections;

namespace LabFusion.Entities;

public class RigGrabber
{
    public class GrabberData
    {
        public Grip Grip;
        public SimpleTransform? TargetInBase = null;
    }

    private readonly RigRefs _references = null;

    private bool _isCulled = false;
    public bool IsCulled => _isCulled;

    private Dictionary<Handedness, GrabberData> _lastGrabs = new();

    public RigGrabber(RigRefs references)
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
            Grip = grip,
            TargetInBase = targetInBase
        };

        if (IsCulled)
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
            {
                return;
            }

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

        if (data.Grip == null)
        {
            _lastGrabs.Remove(handedness);
            return;
        }

        Attach(handedness, data.Grip, data.TargetInBase);
    }

    public void CheckDetachAndReattach(Hand hand, Grip grip)
    {
        DelayUtilities.InvokeNextFrame(OnNextFrame);

        void OnNextFrame()
        {
            if (!ValidateDetach(hand, grip) && hand.AttachedReceiver != grip)
            {
                UncullGrip(hand.handedness);
            }
        }
    }

    private bool ValidateDetach(Hand hand, Grip grip)
    {
        if (IsCulled)
        {
            return true;
        }

        var handedness = hand.handedness;

        if (!_lastGrabs.TryGetValue(handedness, out var existingGrab))
        {
            return true;
        }

        if (existingGrab.Grip == grip)
        {
            return false;
        }

        return true;
    }
}