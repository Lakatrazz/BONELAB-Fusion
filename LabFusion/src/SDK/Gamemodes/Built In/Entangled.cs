using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Scene;
using LabFusion.UI.Popups;
using LabFusion.Menu;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class Entangled : Gamemode
{
    public class EntangledTether
    {
        public PlayerID player1;
        public PlayerID player2;

        public Rigidbody selfPelvis;
        public Rigidbody otherPelvis;
        public ConfigurableJoint joint;

        public GameObject lineInstance;

        public EntangledTether(PlayerID player1, PlayerID player2)
        {
            this.player1 = player1;
            this.player2 = player2;

            FusionSpawnableReferences.EntangledLineReference.Crate.LoadAsset((Action<GameObject>)((prefab) =>
            {
                lineInstance = GameObject.Instantiate(prefab);
                GameObject.DontDestroyOnLoad(lineInstance);
                lineInstance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }));
        }

        public void Dispose()
        {
            if (joint != null)
            {
                GameObject.Destroy(joint);
            }

            if (lineInstance != null)
            {
                GameObject.Destroy(lineInstance);
            }
        }

        public bool IsValid()
        {
            return !PlayerID.IsNullOrInvalid(player1) && !PlayerID.IsNullOrInvalid(player2);
        }

        public void OnUpdate()
        {
            // Make sure we aren't loading
            if (FusionSceneManager.IsLoading() || !IsValid())
                return;

            // Validate pelvis rigidbodies
            if (selfPelvis == null && PlayerRepUtilities.TryGetReferences(player1, out var ref1) && ref1.IsValid)
            {
                selfPelvis = ref1.RigManager.physicsRig.torso._pelvisRb;
            }

            if (otherPelvis == null && PlayerRepUtilities.TryGetReferences(player2, out var ref2) && ref2.IsValid)
            {
                otherPelvis = ref2.RigManager.physicsRig.torso._pelvisRb;
            }

            // If we have both pelvises, update them
            if (selfPelvis != null && otherPelvis != null)
            {
                // Create joint if missing
                if (joint == null)
                {
                    CreateJoint();
                }

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
            else
            {
                lineInstance.SetActive(false);
            }
        }

        private void CreateJoint()
        {
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
    public override string Title => "Entangled";
    public override string Author => FusionMod.ModAuthor;
    public override string Description =>
        "Get tethered to a random player! " +
        "No objectives except to mess around!";
    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

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

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    protected PlayerID _partner = null;
    protected List<EntangledTether> _tethers = new List<EntangledTether>();

    public override void OnGamemodeRegistered()
    {
        Instance = this;

        SetDefaultValues();
    }

    public override void OnMainSceneInitialized()
    {
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        var song = new AudioReference(FusionMonoDiscReferences.GeoGrpFellDownTheStairsReference);
        Playlist.SetPlaylist(song);
    }

    public override void OnGamemodeUnregistered()
    {
        if (Instance == this)
            Instance = null;

        // Cleanup partner
        _partner = null;

        foreach (var tether in _tethers)
        {
            tether.Dispose();
        }

        _tethers.Clear();
    }

    protected override void OnUpdate()
    {
        // Make sure the gamemode is active
        if (!IsStarted)
        {
            return;
        }

        // Update music
        Playlist.Update();

        // Update all tethers
        foreach (var tether in _tethers)
        {
            tether.OnUpdate();
        }
    }

    protected override void OnPlayerJoined(PlayerID id)
    {
        var unassignedPlayers = GetUnassignedPlayers();

        if (unassignedPlayers.Count > 0)
        {
            AssignPartners(unassignedPlayers[0], id);
        }
        else
            AssignPartners(id, null);
    }

    protected override void OnPlayerLeft(PlayerID id)
    {
        RemovePartners(id);
    }

    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        Playlist.StartPlaylist();

        // Recursively assign players until there are no more pairs left
        if (NetworkInfo.IsHost)
        {
            var unassignedPlayers = GetUnassignedPlayers();

            // Make sure there are always at least 2 players
            while (unassignedPlayers.Count > 1)
            {
                unassignedPlayers.Shuffle();

                var player1 = unassignedPlayers[0];
                var player2 = unassignedPlayers[1];

                AssignPartners(player1, player2);

                unassignedPlayers.Remove(player1);
                unassignedPlayers.Remove(player2);
            }

            // Check if there are any left
            if (unassignedPlayers.Count > 0)
            {
                AssignPartners(unassignedPlayers[0], null);
            }
        }
    }

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        Playlist.StopPlaylist();

        // Send notification of gamemode stop
        Notifier.Send(new Notification()
        {
            Title = "Entangled Finish",
            Message = $"The gamemode has ended!",
            SaveToMenu = false,
            ShowPopup = true,
            PopupLength = 2f,
        });

        // Remove all player partnerships
        if (NetworkInfo.IsHost)
        {
            foreach (var player in PlayerIDManager.PlayerIDs)
            {
                RemovePartners(player);
            }
        }

        // Remove self partner
        _partner = null;

        foreach (var tether in _tethers)
        {
            tether.Dispose();
        }

        _tethers.Clear();
    }

    protected List<PlayerID> GetUnassignedPlayers()
    {
        List<PlayerID> unassignedPlayers = new List<PlayerID>();

        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            if (GetPartner(player) == null)
                unassignedPlayers.Add(player);
        }

        return unassignedPlayers;
    }

    protected void AssignPartners(PlayerID player1, PlayerID player2)
    {
        // Check if the second player is null
        if (player2 == null)
        {
            Metadata.TrySetMetadata(GetPartnerKey(player1), "-1");
            return;
        }

        // Set partners both ways
        Metadata.TrySetMetadata(GetPartnerKey(player1), player2.PlatformID.ToString());
        Metadata.TrySetMetadata(GetPartnerKey(player2), player1.PlatformID.ToString());

        // Teleport the first player to the second
        if (NetworkPlayerManager.TryGetPlayer(player2, out var rep) && rep.HasRig)
        {
            PlayerSender.SendPlayerTeleport(player1, rep.RigRefs.RigManager.physicsRig._feetRb.position);
        }
    }

    protected void RemovePartners(PlayerID player1)
    {
        var player2 = GetPartner(player1);

        Metadata.TryRemoveMetadata(GetPartnerKey(player1));

        if (player2 != null)
            Metadata.TryRemoveMetadata(GetPartnerKey(player2));
    }

    protected string GetPartnerKey(PlayerID id)
    {
        return $"{PlayerPartnerKey}.{id.PlatformID}";
    }

    protected PlayerID GetPartner(PlayerID id)
    {
        if (Metadata.TryGetMetadata(GetPartnerKey(id), out var value) && ulong.TryParse(value, out var other))
        {
            return PlayerIDManager.GetPlayerID(other);
        }

        return null;
    }

    protected void OnReceivePartner(PlayerID id)
    {
        if (id == null)
        {
            Notifier.Send(new Notification()
            {
                Title = "Entangled Partner Assignment",
                Message = $"You have no assigned partner! Wait for a new person to join the lobby!",
                SaveToMenu = false,
                ShowPopup = true,
                PopupLength = 5f,
            });

            _partner = null;

            EntangledTether localTether = GetTether(PlayerIDManager.LocalID);

            if (localTether != null)
            {
                localTether.Dispose();
                _tethers.RemoveInstance(localTether);
            }

            return;
        }

        _partner = id;
        _tethers.Add(new EntangledTether(PlayerIDManager.LocalID, id));

        id.TryGetDisplayName(out var name);

        Notifier.Send(new Notification()
        {
            Title = "Entangled Partner Assignment",
            Message = $"Your partner is: {name}",
            SaveToMenu = false,
            ShowPopup = true,
            PopupLength = 5f,
        });
    }

    protected override void OnMetadataChanged(string key, string value)
    {
        // Check if we are being assigned a partner
        if (GetPartnerKey(PlayerIDManager.LocalID) == key)
        {
            if (value == "-1")
            {
                OnReceivePartner(null);
            }
            else if (ulong.TryParse(value, out var partnerId))
            {
                OnReceivePartner(PlayerIDManager.GetPlayerID(partnerId));
            }
        }
        else if (value != "-1" && key.StartsWith(PlayerPartnerKey))
        {
            var id = GetPlayerId(key);

            if (id != null && ulong.TryParse(value, out var partnerId))
            {
                _tethers.Add(new EntangledTether(id, PlayerIDManager.GetPlayerID(partnerId)));
            }
        }
    }

    protected override void OnMetadataRemoved(string key, string value)
    {
        if (GetPartnerKey(PlayerIDManager.LocalID) == key)
        {
            _partner = null;
            var localTether = GetTether(PlayerIDManager.LocalID);

            if (localTether != null)
            {
                localTether.Dispose();
                _tethers.RemoveInstance(localTether);
            }
        }
        else if (key.StartsWith(PlayerPartnerKey))
        {
            var id = GetPlayerId(key);

            if (id != null)
            {
                var tether = GetTether(id);

                if (tether != null)
                {
                    tether.Dispose();
                    _tethers.RemoveInstance(tether);
                }
            }
        }
    }

    protected PlayerID GetPlayerId(string partnerKey)
    {
        foreach (var id in PlayerIDManager.PlayerIDs)
        {
            if (GetPartnerKey(id) == partnerKey)
            {
                return id;
            }
        }

        return null;
    }

    protected EntangledTether GetTether(PlayerID id)
    {
        foreach (var tether in _tethers)
        {
            if (tether.player1 == id || tether.player2 == id)
                return tether;
        }

        return null;
    }
}