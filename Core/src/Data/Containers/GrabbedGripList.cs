using LabFusion.Extensions;

using SLZ.Interaction;

using System.Collections.Generic;

namespace LabFusion.Data {
    public class GrabbedGripList {
        private readonly List<Grip> _grips;

        public GrabbedGripList(int count = 32) {
            _grips = new List<Grip>(count);
        }

        public void OnGripAttach(Grip grip) {
            if (!_grips.Has(grip))
                _grips.Add(grip);
        }
        
        public void OnGripDetach(Grip grip) {
            if (grip.attachedHands.Count <= 0)
                _grips.RemoveInstance(grip);
        }

        public IReadOnlyList<Grip> GetGrabbedGrips() => _grips;
    }
}
