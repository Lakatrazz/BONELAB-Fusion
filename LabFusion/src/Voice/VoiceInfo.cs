using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;

namespace LabFusion.Voice;

public static class VoiceInfo
{
    /// <summary>
    /// The current voice manager. Can be null. Contains information about player voice chat.
    /// </summary>
    public static IVoiceManager VoiceManager => NetworkInfo.CurrentNetworkLayer.VoiceManager;

    public static bool CanTalk => (VoiceManager?.CanTalk).GetValueOrDefault();
    public static bool CanHear => (VoiceManager?.CanHear).GetValueOrDefault();

    public static bool ShowMuteIndicator => NetworkInfo.HasServer && ClientSettings.Muted.Value && ClientSettings.MutedIndicator.Value;

    public static float VoiceAmplitude => (VoiceManager?.GetReceiver()?.GetVoiceAmplitude()).GetValueOrDefault();

    public static bool HasVoiceActivity => (VoiceManager?.GetReceiver()?.HasVoiceActivity()).GetValueOrDefault();

    public static bool IsMuted
    {
        get
        {
            bool isDying = false;

            if (RigData.HasPlayer)
            {
                isDying = RigData.Refs.Health.deathIsImminent;
            }

            return ClientSettings.Muted.Value || isDying;
        }
    }

    public static bool IsDeafened => ClientSettings.Deafened.Value || !ServerVoiceEnabled;

    public static bool IsVoiceEnabled
    {
        get
        {
            bool serverSetting = ServerSettingsManager.ActiveSettings.VoicechatEnabled.Value;
            return serverSetting && !IsMuted && !IsDeafened;
        }
    }

    public static bool ServerVoiceEnabled { get { return ServerSettingsManager.ActiveSettings.VoicechatEnabled.Value; } }
}