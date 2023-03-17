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
            public PlayerId player1;
            public PlayerId player2;

            public Rigidbody selfPelvis;
            public Rigidbody otherPelvis;
            public ConfigurableJoint joint;

            public GameObject lineInstance;

            public EntangledTether(PlayerId player1, PlayerId player2) {
                this.player1 = player1;
                this.player2 = player2;

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

            public bool IsValid() {
                return !PlayerId.IsNullOrInvalid(player1) && !PlayerId.IsNullOrInvalid(player2);
            }

            public void OnUpdate() {
                // Make sure we aren't loading
                if (FusionSceneManager.IsLoading() || !IsValid())
                    return;

                // Validate pelvis rigidbodies
                if (selfPelvis.IsNOC() && PlayerRepUtilities.TryGetReferences(player1, out var ref1) && ref1.IsValid)
                    selfPelvis = ref1.RigManager.physicsRig.torso._pelvisRb;

                if (otherPelvis.IsNOC() && PlayerRepUtilities.TryGetReferences(player2, out var ref2) && ref2.IsValid)
                    otherPelvis = ref2.RigManager.physicsRig.torso._pelvisRb;

                // If we have both pelvises, update them
                if (selfPelvis != null && otherPelvis != null) {
                    // Create joint if missing
                    if (joint.IsNOC())
                        CreateJoint();

                    // Update line renderer
                    lineInstance.SetActive(true);

                    lineInstance.transform.position = selfPelvis.position;

                    Vector3 fromTo = otherPelvis.position - selfPelvis.position;
                    lineInstance.transform.rotation = Quaternion.LookRotation(Vector3Extensions.Normalize(fromTo));

                    Vector3 scale = lineInstance.transform.localScale;
                    scale.z = Vector3Extensions.GetMagnitude(fromTo);
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
                joint.linearLimitSpring = new SoftJointLimitSpring() { spring = 7000f };

                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3Extensions.zero;
                joint.connectedAnchor = Vector3Extensions.zero;

                joint.connectedBody = otherPelvis;
            }
        }

        public static Entangled Instance { get; private set; }

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

        protected PlayerId _partner = null;
        protected List<EntangledTether> _tethers = new List<EntangledTether>();

        private bool _hasOverridenValues = false;

        public override void OnGamemodeRegistered() {
            Instance = this;

            // Add hooks
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;

            SetDefaultValues();
        }

        public override void OnMainSceneInitialized() {
            if (!_hasOverridenValues) {
                SetDefaultValues();
            }
            else {
                _hasOverridenValues = false;
            }
        }

        public void SetDefaultValues() {
            SetPlaylist(DefaultMusicVolume, FusionContentLoader.GeoGrpFellDownTheStairs);
        }

        public void SetOverriden() {
            if (FusionSceneManager.IsLoading()) {
                if (!_hasOverridenValues)
                    SetDefaultValues();

                _hasOverridenValues = true;
            }
        }

        public override void OnLoadingBegin() {
            _hasOverridenValues = false;
        }

        public override void OnGamemodeUnregistered() {
            if (Instance == this)
                Instance = null;

            // Cleanup partner
            _partner = null;

            foreach (var tether in _tethers) {
                tether.Dispose();
            }

            _tethers.Clear();

            // Remove hooks
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
        }

        protected override void OnUpdate() {
            // Update all tethers
            if (IsActive()) {
                foreach (var tether in _tethers) {
                    tether.OnUpdate();
                }
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

            foreach (var tether in _tethers) {
                tether.Dispose();
            }

            _tethers.Clear();
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

                EntangledTether localTether = GetTether(PlayerIdManager.LocalId);

                if (localTether != null) {
                    localTether.Dispose();
                    _tethers.RemoveInstance(localTether);
                }

                return;
            }

            _partner = id;
            _tethers.Add(new EntangledTether(PlayerIdManager.LocalId, id));

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
            else if (value != "-1" && key.StartsWith(PlayerPartnerKey)) {
                var id = GetPlayerId(key);

                if (id != null && ulong.TryParse(value, out var partnerId)) {
                    _tethers.Add(new EntangledTether(id, PlayerIdManager.GetPlayerId(partnerId)));
                }
            }
        }

        protected override void OnMetadataRemoved(string key) {
            if (GetPartnerKey(PlayerIdManager.LocalId) == key) {
                _partner = null;
                var localTether = GetTether(PlayerIdManager.LocalId);

                if (localTether != null) {
                    localTether.Dispose();
                    _tethers.RemoveInstance(localTether);
                }
            }
            else if (key.StartsWith(PlayerPartnerKey)) {
                var id = GetPlayerId(key);

                if (id != null) {
                    var tether = GetTether(id);

                    if (tether != null) {
                        tether.Dispose();
                        _tethers.RemoveInstance(tether);
                    }
                }
            }
        }

        protected PlayerId GetPlayerId(string partnerKey) {
            foreach (var id in PlayerIdManager.PlayerIds) {
                if (GetPartnerKey(id) == partnerKey) {
                    return id;
                }
            }

            return null;
        }

        protected EntangledTether GetTether(PlayerId id) {
            foreach (var tether in _tethers) {
                if (tether.player1 == id || tether.player2 == id)
                    return tether;
            }

            return null;
        }
    }
}
