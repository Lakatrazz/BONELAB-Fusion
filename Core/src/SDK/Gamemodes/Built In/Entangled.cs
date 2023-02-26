using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes {
    public class Entangled : Gamemode {
        public class EntangledTether {
            public Rigidbody selfPelvis;
            public Rigidbody otherPelvis;
            public ConfigurableJoint joint;

            public GameObject lineInstance;

            public EntangledTether() {
                lineInstance = GameObject.Instantiate(FusionContentLoader.EntangledLinePrefab);
                GameObject.DontDestroyOnLoad(lineInstance);
                lineInstance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            public void Dispose() {
                if (!joint.IsNOC())
                    GameObject.Destroy(joint);

                if (!lineInstance.IsNOC())
                    GameObject.Destroy(lineInstance);
            }

            public void OnUpdate(PlayerId partner) {
                // Make sure we aren't loading
                if (LevelWarehouseUtilities.IsLoading())
                    return;

                // Validate pelvis rigidbodies
                if (selfPelvis.IsNOC() && RigData.RigReferences.RigManager != null)
                    selfPelvis = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb;

                if (otherPelvis.IsNOC() && PlayerRepManager.TryGetPlayerRep(partner, out var rep))
                    otherPelvis = rep.RigReferences.RigManager.physicsRig.torso._pelvisRb;

                // If we have both pelvises, update them
                if (selfPelvis != null && otherPelvis != null) {
                    // Create joint if missing
                    if (joint.IsNOC())
                        CreateJoint();

                    // Update line renderer
                    lineInstance.SetActive(true);

                    lineInstance.transform.position = selfPelvis.position;

                    Vector3 fromTo = otherPelvis.position - selfPelvis.position;
                    lineInstance.transform.rotation = Quaternion.LookRotation(fromTo.normalized);

                    Vector3 scale = lineInstance.transform.localScale;
                    scale.z = fromTo.magnitude;
                    lineInstance.transform.localScale = scale;
                }
                // Otherwise, hide the line
                else {
                    lineInstance.SetActive(false);
                }
            }

            private void CreateJoint() {
                joint = selfPelvis.gameObject.AddComponent<ConfigurableJoint>();

                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
                joint.linearLimit = new SoftJointLimit() { limit = 3f };

                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = Vector3.zero;

                joint.connectedBody = otherPelvis;
            }
        }

        // Set info for the gamemode
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Entangled";

        // Set parameters for the gamemode
        public override bool AutoStopOnSceneLoad => false;
        public override bool DisableSpawnGun => false;
        public override bool DisableDevTools => false;
        public override bool DisableManualUnragdoll => false;

        // Metadata
        // Prefix
        public const string DefaultPrefix = "InternalEntangledMetadata";

        // Keys
        public const string PlayerPartnerKey = DefaultPrefix + ".Partner";

        protected PlayerId _partner;
        protected EntangledTether _localTether = null;

        public override void OnGamemodeRegistered() {
            // Add hooks
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;

            SetDefaultValues();
        }

        public override void OnMainSceneInitialized() {
            SetDefaultValues();
        }

        public void SetDefaultValues() {
            SetPlaylist(0.7f, FusionContentLoader.GeoGrpFellDownTheStairs);
        }

        public override void OnGamemodeUnregistered() {
            // Cleanup partner
            _partner = null;

            if (_localTether != null)
                _localTether.Dispose();

            _localTether = null;

            // Remove hooks
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
        }

        protected override void OnUpdate() {
            // Update our self tether
            if (_localTether != null) {
                if (_partner != null && _partner.IsValid)
                    _localTether.OnUpdate(_partner);
                else
                    _localTether.Dispose();
            }
        }

        protected void OnPlayerJoin(PlayerId id) {
            if (IsActive()) {
                var unassignedPlayers = GetUnassignedPlayers();

                if (unassignedPlayers.Count > 0) {
                    AssignPartners(unassignedPlayers[0], id);
                }
                else
                    AssignPartners(id, null);
            }
        }

        protected void OnPlayerLeave(PlayerId id) {
            RemovePartners(id);
        }

        protected override void OnStartGamemode() {
            base.OnStartGamemode();

            // Recursively assign players until there are no more pairs left
            if (NetworkInfo.IsServer) {
                var unassignedPlayers = GetUnassignedPlayers();

                // Make sure there are always at least 2 players
                while (unassignedPlayers.Count > 1) {
                    unassignedPlayers.Shuffle();

                    var player1 = unassignedPlayers[0];
                    var player2 = unassignedPlayers[1];

                    AssignPartners(player1, player2);

                    unassignedPlayers.Remove(player1);
                    unassignedPlayers.Remove(player2);
                }

                // Check if there are any left
                if (unassignedPlayers.Count > 0) {
                    AssignPartners(unassignedPlayers[0], null);
                }
            }
        }

        protected override void OnStopGamemode() {
            base.OnStopGamemode();

            // Send notification of gamemode stop
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Entangled Finish",
                showTitleOnPopup = true,
                message = $"The gamemode has ended!",
                isMenuItem = false,
                isPopup = true,
                popupLength = 2f,
            });

            // Remove all player partnerships
            if (NetworkInfo.IsServer) {
                foreach (var player in PlayerIdManager.PlayerIds) {
                    RemovePartners(player);
                }
            }

            // Remove self partner
            _partner = null;

            if (_localTether != null)
                _localTether.Dispose();

            _localTether = null;
        }

        protected List<PlayerId> GetUnassignedPlayers() {
            List<PlayerId> unassignedPlayers = new List<PlayerId>();

            foreach (var player in PlayerIdManager.PlayerIds) {
                if (GetPartner(player) == null)
                    unassignedPlayers.Add(player);
            }

            return unassignedPlayers;
        }

        protected void AssignPartners(PlayerId player1, PlayerId player2) {
            // Check if the second player is null
            if (player2 == null) {
                TrySetMetadata(GetPartnerKey(player1), "-1");
                return;
            }

            // Set partners both ways
            TrySetMetadata(GetPartnerKey(player1), player2.LongId.ToString());
            TrySetMetadata(GetPartnerKey(player2), player1.LongId.ToString());

            // Teleport the first player to the second
            if (PlayerRepManager.TryGetPlayerRep(player2, out var rep) && rep.IsCreated) {
                PlayerSender.SendPlayerTeleport(player1, rep.RigReferences.RigManager.physicsRig._feetRb.position); 
            }
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

        protected void OnReceivePartner(PlayerId id) {
            if (id == null) {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Entangled Partner Assignment",
                    showTitleOnPopup = true,
                    message = $"You have no assigned partner! Wait for a new person to join the lobby!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                });

                _partner = null;
                if (_localTether != null)
                    _localTether.Dispose();
                _localTether = null;

                return;
            }

            _partner = id;
            _localTether = new EntangledTether();

            id.TryGetDisplayName(out var name);

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Entangled Partner Assignment",
                showTitleOnPopup = true,
                message = $"Your partner is: {name}",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
            });
        }

        protected override void OnMetadataChanged(string key, string value) {
            // Check if we are being assigned a partner
            if (GetPartnerKey(PlayerIdManager.LocalId) == key) {
                if (value == "-1") {
                    OnReceivePartner(null);
                }
                else if (ulong.TryParse(value, out var partnerId)) {
                    OnReceivePartner(PlayerIdManager.GetPlayerId(partnerId));
                }
            }
        }

        protected override void OnMetadataRemoved(string key) {
            if (GetPartnerKey(PlayerIdManager.LocalId) == key) {
                _partner = null;

                if (_localTether != null)
                    _localTether.Dispose();

                _localTether = null;
            }
        }
    }
}
