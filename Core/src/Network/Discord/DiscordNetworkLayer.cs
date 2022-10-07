using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;

using Discord;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class DiscordNetworkLayer : NetworkLayer {
        public const long ApplicationID = 1027381663954645013;

        public Discord.Discord discord;

        public ActivityManager activityManager;
        public LobbyManager lobbyManager;
        public UserManager userManager;
        public ImageManager imageManager;
        public VoiceManager voiceManager;

        public Activity activity;
        public Lobby lobby;

        public User currentUser;

        public override void OnInitializeLayer() {
            // Load our game SDK
            DiscordSDKLoader.OnLoadGameSDK();
        }

        public override void StartServer()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void OnLateInitializeLayer() {
            FusionLogger.Log("Initializing Discord Instance");

            discord = new Discord.Discord(ApplicationID, (long)CreateFlags.Default);

            activityManager = discord.GetActivityManager();

            lobbyManager = discord.GetLobbyManager();

            userManager = discord.GetUserManager();

            userManager.OnCurrentUserUpdate += () =>
            {
                currentUser = userManager.GetCurrentUser();
                FusionLogger.Log($"Current Discord User: {currentUser.Username}");
            };

            imageManager = discord.GetImageManager();

            voiceManager = discord.GetVoiceManager();

            DefaultRichPresence();
        }

        public override void OnUpdateLayer() {
        }

        public override void OnLateUpdateLayer() {
        }

        public override void OnCleanupLayer() {
            discord.Dispose();
            DiscordSDKLoader.OnFreeGameSDK();
        }

        public void DefaultRichPresence()
        {
            activity = new Activity
            {
                Name = "Fusion",
                State = "Playing solo",
                Details = $"Using v{FusionMod.Version}",
                Assets = CreateAssets(),
                Instance = false
            };

            UpdateActivity();
        }

        public ActivityAssets CreateAssets() {
            return new ActivityAssets();
        }

        // Shortcut method for refreshing the user's Discord RP
        public void UpdateActivity() => activityManager.UpdateActivity(activity, (x) => { });
    }
}
