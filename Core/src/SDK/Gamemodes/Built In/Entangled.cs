using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public class Entangled : Gamemode {
        // Set info for the gamemode
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Entangled";

        // Set parameters for the gamemode
        public override bool AutoStopOnSceneLoad => false;
        public override bool ManualPlaylist => true;
        public override bool MusicEnabled => false;
        public override bool DisableSpawnGun => false;
        public override bool DisableDevTools => false;
        public override bool DisableManualUnragdoll => false;

        // Metadata
        // Prefix
        public const string DefaultPrefix = "InternalEntangledMetadata";

        // Keys
        public const string PlayerPartnerKey = DefaultPrefix + ".Partner";

        protected PlayerId _partner;

        public override void OnGamemodeRegistered() {
            // Add hooks
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
        }

        public override void OnGamemodeUnregistered() {
            // Remove hooks
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
        }

        protected void OnPlayerLeave(PlayerId id) {
            RemovePartners(id);
        }

        protected void AssignPartners(PlayerId player1, PlayerId player2) {
            // Set partners both ways
            TrySetMetadata(GetPartnerKey(player1), player2.LongId.ToString());
            TrySetMetadata(GetPartnerKey(player2), player1.LongId.ToString());
        }

        protected void RemovePartners(PlayerId player1) {
            var player2 = GetPartner(player1);

            TryRemoveMetadata(GetPartnerKey(player1));

            if (player2 != null)
                TryRemoveMetadata(GetPartnerKey(player2));
        }

        protected string GetPartnerKey(PlayerId id) {
            return $"{PlayerPartnerKey}.{id.LongId}";
        }

        protected PlayerId GetPartner(PlayerId id) {
            if (TryGetMetadata(GetPartnerKey(id), out var value) && ulong.TryParse(value, out var other)) {
                return PlayerIdManager.GetPlayerId(other);
            }

            return null;
        }

        protected override void OnMetadataChanged(string key, string value) {
            // Check if we are being assigned a partner
            if (GetPartnerKey(PlayerIdManager.LocalId) == key && ulong.TryParse(value, out var partnerId)) {
                _partner = PlayerIdManager.GetPlayerId(partnerId);
            }
        }

        protected override void OnMetadataRemoved(string key) {
            if (GetPartnerKey(PlayerIdManager.LocalId) == key) {
                _partner = null;
            }
        }
    }
}
