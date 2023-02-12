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
using MelonLoader;

namespace LabFusion.Network
{
    public class ConnectionRequestData : IFusionSerializable, IDisposable {
        public ulong longId;
        public Version version;
        public string avatarBarcode;
        public SerializedAvatarStats avatarStats;
        public Dictionary<string, string> initialMetadata;

        public void Serialize(FusionWriter writer) {
            writer.Write(longId);
            writer.Write(version);
            writer.Write(avatarBarcode);
            writer.Write(avatarStats);
            writer.Write(initialMetadata);
        }
        
        public void Deserialize(FusionReader reader) {
            longId = reader.ReadUInt64();
            version = reader.ReadVersion();
            avatarBarcode = reader.ReadString();
            avatarStats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            initialMetadata = reader.ReadStringDictionary();
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
                        PlayerPermissions.FetchPermissionLevel(data.longId, out var level, out _);

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
                        ConnectionSender.SendPlayerJoin(new PlayerId(data.longId, newSmallId.Value, data.initialMetadata), data.avatarBarcode, data.avatarStats);

                        // Now we send all of our other players to the new player
                        foreach (var id in PlayerIdManager.PlayerIds) {
                            var barcode = AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;
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
                        LoadSender.SendLevelLoad(LevelWarehouseUtilities.GetCurrentLevel().Barcode, data.longId);

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

                        // Wait to catchup the user
                        MelonCoroutines.Start(Internal_DelayedCatchup(data.longId));
                    }
                }
            }
        }

        private static IEnumerator Internal_DelayedCatchup(ulong user) {
            // Wait a good amount of time
            for (var i = 0; i < 120; i++) {
                yield return null;
            }

            // Get the player id, check if they're still loading
            var id = PlayerIdManager.GetPlayerId(user);
            if (id != null) {
                while (id.GetMetadata(MetadataHelper.LoadingKey) == bool.TrueString)
                    yield return null;
            }

            // Start to catch them up on the server
            // Catchup the user on synced objects
            foreach (var syncable in SyncManager.Syncables) {
                syncable.Value.InvokeCatchup(user);
            }

            // Catchup hooked events
            MultiplayerHooking.Internal_OnPlayerCatchup(user);
        }
    }
}
