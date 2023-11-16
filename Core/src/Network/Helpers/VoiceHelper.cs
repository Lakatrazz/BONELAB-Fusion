using LabFusion.Data;
using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public static class VoiceHelper
    {
        public static bool CanTalk => (NetworkInfo.VoiceManager?.CanTalk).GetValueOrDefault();
        public static bool CanHear => (NetworkInfo.VoiceManager?.CanHear).GetValueOrDefault();

        public static bool ShowIndicator => NetworkInfo.HasServer && FusionPreferences.ClientSettings.Muted.GetValue() && FusionPreferences.ClientSettings.MutedIndicator.GetValue();

        public static bool IsMuted
        {
            get
            {
                bool isDying = false;
                if (RigData.HasPlayer)
                {
                    isDying = RigData.RigReferences.Health.deathIsImminent;
                }

                return FusionPreferences.ClientSettings.Muted || isDying;
            }
        }

        public static bool IsDeafened => FusionPreferences.ClientSettings.Deafened || !ServerVoiceEnabled;

        public static bool IsVoiceEnabled
        {
            get
            {
                bool serverSetting = FusionPreferences.ActiveServerSettings.VoicechatEnabled.GetValue();
                return serverSetting && !IsMuted && !IsDeafened;
            }
        }

        public static bool ServerVoiceEnabled { get { return FusionPreferences.ActiveServerSettings.VoicechatEnabled.GetValue(); } }
    }
}
