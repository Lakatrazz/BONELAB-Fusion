using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public static class VoteKickHelper
    {
        // The minimum amount of time that can pass before someone can vote again (in seconds)
        public const float MinVoteDelay = 120f;

        // The amount of time that can pass between votes before the total votes are reset.
        public const float ResetVoteTime = 60f;

        // The minimum amount of players needed to vote before a kick is enforced
        public const float RequiredPercent = 0.6f;

        public class VoteKickInfo
        {
            public ulong target;
            public List<ulong> voters;

            public VoteKickInfo(ulong target, ulong voter)
            {
                this.target = target;

                voters = new() {
                    voter
                };
            }
        }

        // Tracks the amount of users in a vote kicking process
        private static readonly FusionDictionary<ulong, VoteKickInfo> _voteKickTracker = new();

        // Tracks the time at which people vote
        private static readonly FusionDictionary<ulong, float> _voterTracker = new();

        private static bool _hasReset = true;
        private static float _timeOfLastVote = -float.PositiveInfinity;

        internal static void Internal_OnInitializeMelon()
        {
            MultiplayerHooking.OnDisconnect += OnResetValues;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnLateUpdate += OnLateUpdate;
        }

        internal static void Internal_OnDeinitializeMelon()
        {
            MultiplayerHooking.OnDisconnect -= OnResetValues;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
        }

        private static void OnLateUpdate()
        {
            // Only check for the server
            if (NetworkInfo.IsServer)
            {
                // Compare times
                if (!_hasReset && TimeUtilities.TimeSinceStartup - _timeOfLastVote >= ResetVoteTime)
                {
                    _hasReset = true;
                    _voteKickTracker.Clear();
                }
            }
        }

        private static void OnResetValues()
        {
            _voteKickTracker.Clear();
            _voterTracker.Clear();

            _hasReset = true;
            _timeOfLastVote = -float.PositiveInfinity;
        }

        private static void OnPlayerLeave(PlayerId player)
        {
            _voteKickTracker.Remove(player.LongId);
            _voterTracker.Remove(player.LongId);
        }

        public static int GetRequiredVotes()
        {
            float count = (float)PlayerIdManager.PlayerCount;
            float percent = RequiredPercent;

            return ManagedMathf.CeilToInt(count * percent);
        }

        public static int GetVoteCount(ulong target)
        {
            if (_voteKickTracker.TryGetValue(target, out var info))
            {
                return info.voters.Count;
            }
            else
                return 0;
        }

        public static bool Vote(ulong target, ulong voter)
        {
            // Make sure the voter hasn't voted in the past specific amount of time
            if (_voterTracker.TryGetValue(voter, out var time) && TimeUtilities.TimeSinceStartup - time < MinVoteDelay)
            {
                return false;
            }
            _voterTracker.Remove(voter);

            // Compare permissions
            FusionPermissions.FetchPermissionLevel(voter, out var voterLevel, out _);
            FusionPermissions.FetchPermissionLevel(target, out var targetLevel, out _);

            if (!FusionPermissions.HasSufficientPermissions(voterLevel, targetLevel))
                return false;

            // Check if the person has already voted on the target
            if (_voteKickTracker.TryGetValue(target, out var info))
            {
                // Add the voter to the info or skip the vote completely
                if (info.voters.Contains(voter))
                    return false;
                else
                    info.voters.Add(voter);
            }
            else
            {
                _voteKickTracker.Add(target, new VoteKickInfo(target, voter));
            }

            // Add the voter to the tracker
            _voterTracker.Add(voter, TimeUtilities.TimeSinceStartup);

            _timeOfLastVote = TimeUtilities.TimeSinceStartup;
            _hasReset = false;

            return true;
        }
    }
}
