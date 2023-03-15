using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.Senders;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using MelonLoader;

namespace LabFusion.Network
{
    public class ConnectionRequestData : IFusionSerializable, IDisposable {
        public ulong longId;
        public Version version;
        public string avatarBarcode;
        public SerializedAvatarStats avatarStats;
        public Dictionary<string, string> initialMetadata;
        public List<string> initialEquippedItems;

        public void Serialize(FusionWriter writer) {
            writer.Write(longId);
            writer.Write(version);
            writer.Write(avatarBarcode);
            writer.Write(avatarStats);
            writer.Write(initialMetadata);
            writer.Write(initialEquippedItems);
        }
        
        public void Deserialize(FusionReader reader) {
            longId = reader.ReadUInt64();
            version = reader.ReadVersion();
            avatarBarcode = reader.ReadString();
            avatarStats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            initialMetadata = reader.ReadStringDictionary();
            initialEquippedItems = reader.ReadStrings().ToList();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static ConnectionRequestData Create(ulong longId, Version version, string avatarBarcode, SerializedAvatarStats stats) {
            return new ConnectionRequestData() {
                longId = longId,
                version = version,
                avatarBarcode = avatarBarcode,
                avatarStats = stats,
                initialMetadata = InternalServerHelpers.GetInitialMetadata(),
                initialEquippedItems = InternalServerHelpers.GetInitialEquippedItems(),
            };
        }
    }

    public class ConnectionRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConnectionRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsServer) {
                using (FusionReader reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<ConnectionRequestData>();
                    var newSmallId = PlayerIdManager.GetUnusedPlayerId();

                    if (PlayerIdManager.GetPlayerId(data.longId) == null && newSmallId.HasValue) {
                        // Check if theres too many players
                        if (PlayerIdManager.PlayerCount >= byte.MaxValue || PlayerIdManager.PlayerCount >= FusionPreferences.LocalServerSettings.MaxPlayers.GetValue()) {
                            ConnectionSender.SendConnectionDeny(data.longId, "Server is full! Wait for someone to leave.");
                            return;
                        }

                        // Make sure we aren't loading
                        if (FusionSceneManager.IsLoading()) {
                            ConnectionSender.SendConnectionDeny(data.longId, "Host is loading.");
                            return;
                        }

                        // Verify joining
                        bool isVerified = NetworkVerification.IsClientApproved(data.longId);

                        if (!isVerified) {
                            ConnectionSender.SendConnectionDeny(data.longId, "Server is private.");
                            return;
                        }

                        // Compare versions
                        VersionResult versionResult = NetworkVerification.CompareVersion(FusionMod.Version, data.version);

                        if (versionResult != VersionResult.Ok) {
                            switch (versionResult) {
                                default:
                                case VersionResult.Unknown:
                                    ConnectionSender.SendConnectionDeny(data.longId, "Unknown Version Mismatch");
                                    break;
                                case VersionResult.Lower:
                                    ConnectionSender.SendConnectionDeny(data.longId, "Server is on an older version. Downgrade your version or notify the host.");
                                    break;
                                case VersionResult.Higher:
                                    ConnectionSender.SendConnectionDeny(data.longId, "Server is on a newer version. Update your version.");
                                    break;
                            }

                            return;
                        }

                        // Get the permission level
                        FusionPermissions.FetchPermissionLevel(data.longId, out var level, out _);

                        // Check for banning
                        if (NetworkHelper.IsBanned(data.longId)) {
                            ConnectionSender.SendConnectionDeny(data.longId, "Banned from Server");
                            return;
                        }

                        // Finally, check for dynamic connection disallowing
                        if (!MultiplayerHooking.Internal_OnShouldAllowConnection(data.longId, out string reason)) {
                            ConnectionSender.SendConnectionDeny(data.longId, reason);
                            return;
                        }

                        // Append metadata with info
                        if (!data.initialMetadata.ContainsKey(MetadataHelper.PermissionKey))
                            data.initialMetadata.Add(MetadataHelper.PermissionKey, level.ToString());
                        else
                            data.initialMetadata[MetadataHelper.PermissionKey] = level.ToString();

                        // First we send the new player to all existing players (and the new player so they know they exist)
                        ConnectionSender.SendPlayerJoin(new PlayerId(data.longId, newSmallId.Value, data.initialMetadata, data.initialEquippedItems), data.avatarBarcode, data.avatarStats);

                        // Now we send all of our other players to the new player
                        foreach (var id in PlayerIdManager.PlayerIds) {
                            var barcode = CommonBarcodes.INVALID_AVATAR_BARCODE;
                            SerializedAvatarStats stats = new SerializedAvatarStats();
                            if (id.SmallId == 0) {
                                barcode = RigData.RigAvatarId;
                                stats = RigData.RigAvatarStats;
                            }
                            else if (PlayerRepManager.TryGetPlayerRep(id.SmallId, out var rep)) {
                                barcode = rep.avatarId;
                                stats = rep.avatarStats;
                            }

                            ConnectionSender.SendPlayerCatchup(data.longId, id, barcode, stats);
                        }

                        // Now, make sure the player loads into the scene
                        LoadSender.SendLevelLoad(FusionSceneManager.Barcode, data.longId);

                        // Send the dynamics list
                        using (var writer = FusionWriter.Create()) {
                            using (var assignData = DynamicsAssignData.Create()) {
                                writer.Write(assignData);

                                using (var message = FusionMessage.Create(NativeMessageTag.DynamicsAssignment, writer)) {
                                    MessageSender.SendFromServer(data.longId, NetworkChannel.Reliable, message);
                                }
                            }
                        }

                        // Send the active server settings
                        FusionPreferences.SendServerSettings(data.longId);

                        // SERVER CATCHUP
                        // Start to catch them up on the server
                        // Catchup the user on synced objects
                        foreach (var syncable in SyncManager.Syncables) {
                            try {
                                syncable.Value.InvokeCatchup(data.longId);
                            }
                            catch (Exception e) {
                                FusionLogger.LogException("sending catchup for syncable", e);
                            }
                        }

                        // Catchup hooked events
                        MultiplayerHooking.Internal_OnPlayerCatchup(data.longId);
                    }
                }
            }
        }
    }
}
