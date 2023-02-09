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
            if (NetworkInfo.CurrentNetworkLayer.IsServer) {
                using (FusionReader reader = FusionReader.Create(bytes)) {
                    var data = reader.ReadFusionSerializable<ConnectionRequestData>();
                    var newSmallId = PlayerIdManager.GetUnusedPlayerId();

                    if (PlayerIdManager.GetPlayerId(data.longId) == null && newSmallId.HasValue) {
                        // Verify joining
                        bool isVerified = NetworkVerification.IsClientApproved(data.longId);

                        if (!isVerified)
                            return;

                        // Compare versions
                        VersionResult versionResult = NetworkVerification.CompareVersion(FusionMod.Version, data.version);

                        if (versionResult != VersionResult.Ok) {
                            return;
                        }

                        // Append metadata with info
                        // Permissions
                        PlayerPermissions.FetchPermissionLevel(data.longId, out var level, out _);

                        if (!data.initialMetadata.ContainsKey(MetadataHelper.PermissionKey))
                            data.initialMetadata.Add(MetadataHelper.PermissionKey, level.ToString());
                        else
                            data.initialMetadata[MetadataHelper.PermissionKey] = level.ToString();

#if DEBUG
                        FusionLogger.Log($"Server received user with long id {data.longId}. Assigned small id {newSmallId}");
#endif

                        // First we send the new player to all existing players (and the new player so they know they exist)
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (var response = ConnectionResponseData.Create(data.longId, newSmallId.Value, data.avatarBarcode, data.avatarStats, data.initialMetadata)) {
                                writer.Write(response);

                                using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                                }
                            }
                        }

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

                            using (FusionWriter writer = FusionWriter.Create()) {
                                using (var response = ConnectionResponseData.Create(id.LongId, id.SmallId, barcode, stats, id.Metadata)) {
                                    writer.Write(response);

                                    using (var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer)) {
                                        MessageSender.SendFromServer(data.longId, NetworkChannel.Reliable, message);
                                    }
                                }
                            }
                        }

                        // Now, make sure the player loads into the scene
                        LoadSender.SendLevelLoad(LevelWarehouseUtilities.GetCurrentLevel().Barcode, data.longId);

                        // Send the module list
                        using (var writer = FusionWriter.Create()) {
                            using (var assignData = ModuleAssignData.Create()) {
                                writer.Write(assignData);

                                using (var message = FusionMessage.Create(NativeMessageTag.ModuleAssignment, writer)) {
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
