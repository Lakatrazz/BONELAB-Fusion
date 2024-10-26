using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences.Server;
using LabFusion.Preferences.Client;

namespace LabFusion.Voice;

public static class VoiceInfo
{
    /// <summary>
    /// The current voice manager. Can be null. Contains information about player voice chat.
    /// </summary>
    public static IVoiceManager VoiceManager => NetworkInfo.CurrentNetworkLayer?.VoiceManager;

    /// <summary>
    /// Returns if the voice manager supports speaking.
    /// </summary>
    public static bool CanTalk => (VoiceManager?.CanTalk).GetValueOrDefault();
    
    /// <summary>
    /// Returns if the voice manager supports listening.
    /// </summary>
    public static bool CanHear => (VoiceManager?.CanHear).GetValueOrDefault();
    
    /// <summary>
    /// Returns an array of all input devices that are available.
    /// </summary>
    public static string[] InputDevices => VoiceManager != null ? VoiceManager.InputDevices : Array.Empty<string>();

    /// <summary>
    /// Returns if the mute icon is enabled.
    /// </summary>
    public static bool ShowMuteIndicator => NetworkInfo.HasServer && ClientSettings.VoiceChat.Muted.Value && ClientSettings.VoiceChat.MutedIndicator.Value;

    /// <summary>
    /// Returns the microphone amplitude for this frame.
    /// </summary>
    public static float VoiceAmplitude => (VoiceManager?.GetReceiver()?.GetVoiceAmplitude()).GetValueOrDefault();

    /// <summary>
    /// Returns if we have voice activity for this frame.
    /// </summary>
    public static bool HasVoiceActivity => (VoiceManager?.GetReceiver()?.HasVoiceActivity()).GetValueOrDefault();

    /// <summary>
    /// Returns if the player can't speak (muted, deafened, or dying).
    /// </summary>
    public static bool IsMuted
    {
        get
        {
            bool isDying = false;

            if (RigData.HasPlayer)
            {
                isDying = RigData.Refs.Health.deathIsImminent;
            }

            return ClientSettings.VoiceChat.Muted.Value || isDying || IsDeafened;
        }
    }

    /// <summary>
    /// Returns if voice chat is currently disabled, either via deafening or the server setting.
    /// </summary>
    public static bool IsDeafened => ClientSettings.VoiceChat.Deafened.Value || !ServerVoiceEnabled;

    /// <summary>
    /// Returns if voice chat is enabled on the server's end.
    /// </summary>
    public static bool ServerVoiceEnabled { get { return LobbyInfoManager.LobbyInfo.VoiceChat; } }
}