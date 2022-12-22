using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public struct GripPair {
        public readonly Hand hand;
        public readonly Grip grip;

        public GripPair(Hand hand, Grip grip) {
            this.hand = hand;
            this.grip = grip;
        }
    }
}
