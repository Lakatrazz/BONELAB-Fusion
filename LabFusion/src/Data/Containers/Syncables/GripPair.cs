using Il2CppSLZ.Interaction;

namespace LabFusion.Data
{
    public struct GripPair
    {
        public readonly Hand hand;
        public readonly Grip grip;

        public GripPair(Hand hand, Grip grip)
        {
            this.hand = hand;
            this.grip = grip;
        }
    }
}
