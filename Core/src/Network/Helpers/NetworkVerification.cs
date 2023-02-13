using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public enum VersionResult {
        Unknown = 0,
        Ok = 1,
        Lower = 2,
        Higher = 3,
    }

    /// <summary>
    /// Helper class for verifying users and actions.
    /// </summary>
    public static class NetworkVerification {
        /// <summary>
        /// Compares the server and user versions.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static VersionResult CompareVersion(Version server, Version user) {
            int serverSum = int.Parse($"{server.Major}{server.Minor}");
            int userSum = int.Parse($"{user.Major}{user.Minor}");

            if (serverSum < userSum)
                return VersionResult.Lower;
            else if (serverSum > userSum)
                return VersionResult.Higher;
            
            return VersionResult.Ok;
        }

        /// <summary>
        /// Returns true if the client is approved to join this server.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsClientApproved(ulong userId) {
            var privacy = FusionPreferences.LocalServerSettings.Privacy.GetValue();

            switch (privacy) {
                default:
                case ServerPrivacy.LOCKED:
                    return false;
                case ServerPrivacy.PUBLIC:
                case ServerPrivacy.PRIVATE:
                    return true;
                case ServerPrivacy.FRIENDS_ONLY:
                    return NetworkHelper.IsFriend(userId);
            }
        }
    }
}
