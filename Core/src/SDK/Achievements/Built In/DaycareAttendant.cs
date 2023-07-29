using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class DaycareAttendant : Achievement
    {
        public override string Title => "Daycare Attendant";

        public override string Description => "Stay in the same server for one hour.";

        public override int BitReward => 500;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(DaycareAttendant)).Preview;

        protected float _timeElapsed;
        protected bool _oneHourPassed = false;

        protected override void OnRegister() {
            MultiplayerHooking.OnJoinServer += OnJoinServer;
            MultiplayerHooking.OnStartServer += OnJoinServer;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        protected override void OnUnregister() {
            MultiplayerHooking.OnJoinServer -= OnJoinServer;
            MultiplayerHooking.OnStartServer -= OnJoinServer;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;

            // Incase it wasn't removed
            MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
        }

        private void OnJoinServer() {
            _timeElapsed = 0f;
            _oneHourPassed = false;

            MultiplayerHooking.OnLateUpdate += OnLateUpdate;
        }

        private void OnDisconnect() {
            _timeElapsed = 0f;
            _oneHourPassed = false;

            MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
        }

        private void OnLateUpdate() { 
            // If we haven't already given the achievement, and there is more than 1 player, increment the timer
            if (!_oneHourPassed && PlayerIdManager.HasOtherPlayers) {
                _timeElapsed += Time.deltaTime;

                // 3600 seconds in an hour
                if (_timeElapsed >= 3600f) {
                    _oneHourPassed = true;
                    IncrementTask();
                }
            }
        }
    }
}
