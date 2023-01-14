using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    /// <summary>
    /// Helper class for verifying users and actions.
    /// </summary>
    public static class NetworkVerification {
        /// <summary>
        /// Returns true if the client is approved to join this server.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsClientApproved(ulong userId) {
            var privacy = FusionPreferences.ServerSettings.Privacy.GetValue();

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
