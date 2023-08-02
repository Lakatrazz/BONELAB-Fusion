﻿using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class BouncingStrong : Achievement
    {
        public override string Title => "Bouncing Strong";

        public override string Description => "Jump as Strong 1000 times across servers.";

        public override int BitReward => 1000;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(BouncingStrong)).Preview;

        public override int MaxTasks => 1000;

        protected override void OnRegister()
        {
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        }

        protected override void OnUnregister()
        {
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        }

        protected override void OnComplete() {
            FusionAudio.Play2D(FusionContentLoader.BouncingStrong, 0.6f);
        }

        private void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer) {
            // Make sure there's other players
            if (!PlayerIdManager.HasOtherPlayers)
                return;

            // Make sure this is us, and that we jumped
            if (player.IsSelf && type == PlayerActionType.JUMP) {
                // Check current avatar
                if (RigData.RigAvatarId == CommonBarcodes.STRONG_BARCODE) {
                    IncrementTask();
                }
            }
        }
    }
}
