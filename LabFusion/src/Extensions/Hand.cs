using Il2CppSLZ.Marrow;

using LabFusion.Entities;

namespace LabFusion.Extensions;

public static class HandExtensions
{
    public static void TryAutoHolsterGrip(this Hand hand, RigRefs collection)
    {
        if (hand.m_CurrentAttachedGO == null)
        {
            return;
        }

        var grip = Grip.Cache.Get(hand.m_CurrentAttachedGO);

        if (grip != null)
        {
            grip.TryAutoHolster(collection);
        }
    }

    public static void TryDetach(this Hand hand)
    {
        var attachedGo = hand.m_CurrentAttachedGO;

        if (attachedGo == null)
        {
            return;
        }

        var grip = attachedGo.GetComponent<Grip>();

        if (grip != null)
        {
            grip.TryDetach(hand);
        }
    }
}