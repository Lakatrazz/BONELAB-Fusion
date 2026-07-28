using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Extensions;
using LabFusion.Marrow.Interaction;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Entities;

public class RigGrabber
{
    public bool IsCulled { get; private set; } = false;

    public Dictionary<Handedness, GrabSnapshot> ReceivedGrabs { get; } = new();

    private readonly RigRefs _references = null;

    public RigGrabber(RigRefs references)
    {
        _references = references;
    }

    public void OnGrabReceived(SerializedGrab grab)
    {
        SetGrab(grab);

        TryReattachGrip(grab.Handedness);
    }

    public void OnReleaseReceived(Handedness handedness)
    {
        ClearGrab(handedness);

        DetachGrip(handedness);
    }

    public void OnEntityCull(bool isInactive)
    {
        IsCulled = isInactive;

        if (isInactive)
        {
            DetachGrips();
        }
        else
        {
            MelonCoroutines.Start(CoWaitAndReattachGrips());
        }
    }

    public void OnRigOwnershipTransfer(bool isOwner)
    {
        if (isOwner)
        {
            ClearGrabs();
        }
    }

    public void ReattachGrips()
    {
        TryReattachGrip(Handedness.LEFT);
        TryReattachGrip(Handedness.RIGHT);
    }

    public bool TryReattachGrip(Handedness handedness)
    {
        if (!ReceivedGrabs.TryGetValue(handedness, out var grabSnapshot))
        {
            return false;
        }

        if (!grabSnapshot.TryGetGrip(out var grip))
        {
            return false;
        }

        AttachGrip(handedness, grip, grabSnapshot.TargetInBase);
        return true;
    }

    public void AttachGrip(Handedness handedness, Grip grip, SimpleTransform? targetInBase = null)
    {
        if (IsCulled)
        {
            return;
        }

        var hand = _references.GetHand(handedness);

        if (hand == null)
        {
            return;
        }

        if (grip == null)
        {
            return;
        }

        // Detach existing grip
        hand.TryDetach();

        bool interactionDisabled = grip.IsInteractionDisabled || (grip.HasHost && grip.Host.IsInteractionDisabled);

        if (interactionDisabled)
        {
            return;
        }

        // Attach the hand
        grip.TryAttach(hand, false, targetInBase);
    }

    public void DetachGrip(Handedness handedness)
    {
        var hand = _references.GetHand(handedness);

        if (hand == null)
        {
            return;
        }

        hand.TryDetach();
    }

    public void DetachGrips()
    {
        DetachGrip(Handedness.LEFT);
        DetachGrip(Handedness.RIGHT);
    }

    public void ValidateDetach(Hand hand, Grip grip)
    {
        DelayUtilities.InvokeNextFrame(OnNextFrame);

        void OnNextFrame()
        {
            if (!CanDetach(hand, grip) && hand.AttachedReceiver != grip)
            {
                TryReattachGrip(hand.handedness);
            }
        }
    }

    public bool CanDetach(Hand hand, Grip grip)
    {
        if (IsCulled)
        {
            return true;
        }

        var handedness = hand.handedness;

        if (!ReceivedGrabs.TryGetValue(handedness, out var existingGrab))
        {
            return true;
        }

        if (existingGrab.Grip == grip)
        {
            return false;
        }

        return true;
    }

    private void SetGrab(SerializedGrab grab)
    {
        ReceivedGrabs[grab.Handedness] = new GrabSnapshot(grab);
    }

    private void ClearGrab(Handedness handedness)
    {
        ReceivedGrabs.Remove(handedness);
    }

    private void ClearGrabs()
    {
        ReceivedGrabs.Clear();
    }

    private IEnumerator CoWaitAndReattachGrips()
    {
        for (var i = 0; i < 120; i++)
        {
            yield return null;
        }

        ReattachGrips();
    }
}