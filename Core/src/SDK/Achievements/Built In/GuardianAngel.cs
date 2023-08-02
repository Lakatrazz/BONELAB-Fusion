﻿using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class GuardianAngel : Achievement
    {
        public override string Title => "Guardian Angel";

        public override string Description => "Save another person from dying.";

        public override int BitReward => 500;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(GuardianAngel)).Preview;

        protected override void OnRegister() {
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        }

        protected override void OnUnregister() {
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        }

        private void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer) {
            // Was the person saved?
            if (!player.IsSelf && type == PlayerActionType.RECOVERY) {
                // Check the most recently killed NPC
                // If we are the owner, we probably saved them
                if (PuppetMasterExtender.LastKilled != null && PuppetMasterExtender.LastKilled.IsOwner()) {
                    IncrementTask();
                }
            }
        }
    }
}
