using LabFusion.Extensions;

using SLZ.Interaction;

using System.Collections.Generic;

namespace LabFusion.Data {
    public class GrabbedGripList {
        private readonly List<Grip> _grips;

        private bool _hasGrabbedGrips = false;
        public bool HasGrabbedGrips => _hasGrabbedGrips;

        public GrabbedGripList(int count = 32) {
            _grips = new List<Grip>(count);
        }

        public void OnPushUpdate() {
            // Check for seats
            foreach (var grip in _grips.ToArray()) {
                foreach (var hand in grip.attachedHands) {
                    if (hand.manager.activeSeat) {
                        _grips.RemoveInstance(grip);
                        break;
                    }
                }
            }

            // Update grip info
            if (_grips.Count <= 0)
                _hasGrabbedGrips = false;
        }

        public void OnGripAttach(Hand hand, Grip grip) {
            if (!_grips.Has(grip) && !hand.manager.activeSeat) {
                _grips.Add(grip);
                _hasGrabbedGrips = true;
            }
        }
        
        public void OnGripDetach(Grip grip) {
            if (grip.attachedHands.Count <= 0)
                _grips.RemoveInstance(grip);

            if (_grips.Count <= 0)
                _hasGrabbedGrips = false;
        }

        public IReadOnlyList<Grip> GetGrabbedGrips() => _grips;
    }
}
