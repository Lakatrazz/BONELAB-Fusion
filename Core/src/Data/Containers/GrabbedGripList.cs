using LabFusion.Extensions;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Interaction;

using System.Collections.Generic;

namespace LabFusion.Data {
    public class GrabbedGripList {
        private readonly Dictionary<Grip, int> _grips;

        private PropSyncable _syncable;

        public GrabbedGripList(PropSyncable syncable, int count = 32) {
            _syncable = syncable;
            _grips = new Dictionary<Grip, int>(count, new UnityComparer());
        }

        public void OnPushUpdate() {
            // Check for seats
            List<Grip> gripsToRemove = null;

            foreach (var grip in _grips.Keys) {
                var hands = grip.attachedHands;
                for (var i = 0; i < hands.Count; i++) {
                    var hand = hands[i];

                    if (hand.manager.activeSeat) {
                        if (gripsToRemove == null)
                            gripsToRemove = new List<Grip>();

                        gripsToRemove.Add(grip);
                        break;
                    }
                }
            }

            // Remove all grips
            if (gripsToRemove != null) {
                for (var i = 0; i < gripsToRemove.Count; i++) {
                    _grips.Remove(gripsToRemove[i]);
                }
            }

            // Update grip info
            if (_grips.Count <= 0)
                _syncable.IsHeld = false;
        }

        public void OnGripAttach(Hand hand, Grip grip) {
            // Make sure the hand wasn't already attached
            if (grip.attachedHands.Has(hand))
                return;

            if (!hand.manager.activeSeat) {
                if (!_grips.ContainsKey(grip)) {
                    _grips.Add(grip, 0);
                }

                _grips[grip]++;
                _syncable.IsHeld = true;
            }
        }
        
        public void OnGripDetach(Hand hand, Grip grip) {
            // Make sure the hand was actually attached when detaching
            if (!grip.attachedHands.Has(hand))
                return;

            if (_grips.ContainsKey(grip)) {
                _grips[grip]--;

                if (_grips[grip] <= 0)
                    _grips.Remove(grip);
            }

            if (_grips.Count <= 0)
                _syncable.IsHeld = false;
        }

        public IReadOnlyCollection<Grip> GetGrabbedGrips() => _grips.Keys;
    }
}
