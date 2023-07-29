using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Senders {
    public enum PlayerActionType {
        /// <summary>
        /// No event.
        /// </summary>
        UNKNOWN = 1 << 0,

        /// <summary>
        /// Invoked when Player jumps.
        /// </summary>
        JUMP = 1 << 1,

        /// <summary>
        /// Invoked when Player dies.
        /// </summary>
        DEATH = 1 << 2,

        /// <summary>
        /// Invoked when Player plays the dying animation.
        /// </summary>
        DYING = 1 << 3,

        /// <summary>
        /// Invoked when Player deals damage to Other Player.
        /// </summary>
        DEALT_DAMAGE_TO_OTHER_PLAYER = 1 << 4,

        /// <summary>
        /// Invoked when Player plays the dying animation due to Other Player.
        /// </summary>
        DYING_BY_OTHER_PLAYER = 1 << 5,

        /// <summary>
        /// Invoked when Player saves themselves before the dying animation ends.
        /// </summary>
        RECOVERY = 1 << 6,

        /// <summary>
        /// Invoked when Player is killed by Other Player.
        /// </summary>
        DEATH_BY_OTHER_PLAYER = 1 << 7,
    }

    public enum NicknameVisibility {
        SHOW = 1 << 0,
        SHOW_WITH_PREFIX = 1 << 1,
        HIDE = 1 << 2,
    }

    public static class PlayerSender {
        public static void SendPlayerAvatar(SerializedAvatarStats stats, string barcode) {
            if (!NetworkInfo.HasServer)
                return;

            using FusionWriter writer = FusionWriter.Create(PlayerRepAvatarData.DefaultSize + barcode.GetSize());
            using PlayerRepAvatarData data = PlayerRepAvatarData.Create(PlayerIdManager.LocalSmallId, stats, barcode);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepAvatar, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }

        public static void SendPlayerVoiceChat(byte[] voiceData, bool layerCompressed) {
            if (!NetworkInfo.HasServer)
                return;

            using var writer = FusionWriter.Create(PlayerVoiceChatData.Size + voiceData.Length);
            using var data = PlayerVoiceChatData.Create(PlayerIdManager.LocalSmallId, voiceData, layerCompressed);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerVoiceChat, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.VoiceChat, message);
        }

        public static void SendPlayerTeleport(byte target, Vector3 position)
        {
            if (!NetworkInfo.IsServer)
                return;

            using var writer = FusionWriter.Create(PlayerRepTeleportData.Size);
            using var data = PlayerRepTeleportData.Create(target, position);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepTeleport, writer);
            MessageSender.SendFromServer(target, NetworkChannel.Reliable, message);
        }

        public static void SendPlayerDamage(byte target, float damage) {
            using var writer = FusionWriter.Create(PlayerRepDamageData.Size);
            using var data = PlayerRepDamageData.Create(PlayerIdManager.LocalSmallId, target, damage);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepDamage, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendPlayerMetadataRequest(byte smallId, string key, string value) {
            using var writer = FusionWriter.Create(PlayerMetadataRequestData.GetSize(key, value));
            using var data = PlayerMetadataRequestData.Create(smallId, key, value);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendPlayerMetadataResponse(byte smallId, string key, string value) {
            // Make sure this is the server
            if (NetworkInfo.IsServer) {
                using var writer = FusionWriter.Create(PlayerMetadataResponseData.GetSize(key, value));
                using var data = PlayerMetadataResponseData.Create(smallId, key, value);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataResponse, writer);
                MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            }
            else
                throw new ExpectedClientException();
        }

        public static void SendVoteKickRequest(byte target) {
            using var writer = FusionWriter.Create(VoteKickRequestData.Size);
            using var data = VoteKickRequestData.Create(PlayerIdManager.LocalId, target);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.VoteKickRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendPlayerAction(PlayerActionType type, byte? otherPlayer = null) {
            using var writer = FusionWriter.Create(PlayerRepActionData.Size);
            using var data = PlayerRepActionData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepAction, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}
