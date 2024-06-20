using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Data;
using LabFusion.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public class RigGrabber
{
    private RigReferenceCollection _references = null;

    private bool _isCulled = false;

    public RigGrabber(RigReferenceCollection references)
    {
        _references = references;
    }

    public void OnEntityCull(bool isInactive)
    {
        _isCulled = isInactive;

        if (isInactive)
        {
            Detach(Handedness.LEFT);
            Detach(Handedness.RIGHT);
        }
    }

    public void Attach(Handedness handedness, Grip grip, SimpleTransform? targetInBase = null)
    {
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
        var hand = _references.GetHand(handedness);

        if (hand == null)
        {
            return;
        }

        hand.TryDetach();
    }
}