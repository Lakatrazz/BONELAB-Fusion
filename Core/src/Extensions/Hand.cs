using LabFusion.Data;
using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class HandExtensions {
        public static void TryAutoHolsterGrip(this Hand hand, RigReferenceCollection collection) {
            if (hand.m_CurrentAttachedGO == null)
                return;

            var grip = Grip.Cache.Get(hand.m_CurrentAttachedGO);

            if (grip != null)
                grip.TryAutoHolster(collection);
        }

        public static void TryDetach(this Hand hand) {
            if (hand.m_CurrentAttachedGO != null) {
                var grip = Grip.Cache.Get(hand.m_CurrentAttachedGO);

                if (grip != null)
                    grip.TryDetach(hand);
            }
        }
    }
}
