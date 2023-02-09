using LabFusion.Extensions;

using SLZ.Interaction;

using System.Collections.Generic;

namespace LabFusion.Data {
    public class GrabbedGripList {
        private readonly Dictionary<Grip, int> _grips;

        private bool _hasGrabbedGrips = false;
        public bool HasGrabbedGrips => _hasGrabbedGrips;

        public GrabbedGripList(int count = 32) {
            _grips = new Dictionary<Grip, int>(count, new UnityComparer());
        }

        public void OnPushUpdate() {
            // Check for seats
            foreach (var grip in _grips.Keys) {
                foreach (var hand in grip.attachedHands) {
                    if (hand.manager.activeSeat) {
                        _grips.Remove(grip);
                        break;
                    }
                }
            }

            // Update grip info
            if (_grips.Count <= 0)
                _hasGrabbedGrips = false;
        }

        public void OnGripAttach(Hand hand, Grip grip) {
            if (!hand.manager.activeSeat) {
                if (!_grips.ContainsKey(grip)) {
                    _grips.Add(grip, 0);
                }

                _grips[grip]++;
                _hasGrabbedGrips = true;
            }
        }
        
        public void OnGripDetach(Grip grip) {
            if (_grips.ContainsKey(grip)) {
                _grips[grip]--;

                if (_grips[grip] <= 0)
                    _grips.Remove(grip);
            }

            if (_grips.Count <= 0)
                _hasGrabbedGrips = false;
        }

        public IReadOnlyCollection<Grip> GetGrabbedGrips() => _grips.Keys;
    }
}
