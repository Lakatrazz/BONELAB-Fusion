using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;

using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network.EpicGames;

internal class EOSManager
{
    internal EOSManager(EOSAuthManager eosAuthManager)
    {
        authManager = eosAuthManager;
    }

    private EOSAuthManager authManager;

    internal static PlatformInterface PlatformInterface;
    internal static ConnectInterface ConnectInterface;
    internal static P2PInterface P2PInterface;
    internal static LobbyInterface LobbyInterface;

    private IEnumerator Ticker()
    {
        float timePassed = 0f;
        while (PlatformInterface != null)
        {
            timePassed += TimeUtilities.UnscaledDeltaTime;
            if (timePassed >= 1f / 20f)
            {
                timePassed = 0f;
                PlatformInterface?.Tick();
            }
            yield return null;
        }

        yield break;
    }

    internal IEnumerator InitializeAsync(System.Action<bool> onComplete)
    {
        if (!InitializeInterfaces())
        {
            onComplete?.Invoke(false);
            yield break;
        }
        
        ConfigureP2P();

        MelonCoroutines.Start(Ticker());

        bool loginComplete = false;
        bool loginSuccess = false;

        MelonCoroutines.Start(authManager.LoginAsync((success) =>
        {
            loginSuccess = success;
            loginComplete = true;
        }));

        while (!loginComplete)
            yield return null;

        if (!loginSuccess)
        {
            ShutdownEOS();
            onComplete?.Invoke(false);
            yield break;
        }

        onComplete.Invoke(true);
    }

    private bool InitializeInterfaces()
    {
        InitializeOptions initializeOptions = new InitializeOptions();

        initializeOptions.ProductName = EOSAuthCredentials.ProductName;
        initializeOptions.ProductVersion = EOSAuthCredentials.ProductVersion;

        Result initializeResult = PlatformInterface.Initialize(ref initializeOptions);

        if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
        {
            FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
            return false;
        }

        LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
        LoggingInterface.SetCallback((ref LogMessage message) =>
        {
            FusionLogger.Log("EOS -> " + message.Message);
        });

        Options options = new Options()
        {
            ProductId = EOSAuthCredentials.ProductId,
            SandboxId = EOSAuthCredentials.SandboxId,
            DeploymentId = EOSAuthCredentials.DeploymentId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = EOSAuthCredentials.ClientId,
                ClientSecret = EOSAuthCredentials.ClientSecret
            },
            Flags = PlatformFlags.DisableOverlay | PlatformFlags.DisableSocialOverlay
        };

        PlatformInterface = PlatformInterface.Create(ref options);
        if (PlatformInterface == null)
        {
            FusionLogger.Error("Failed to create EOS Platform Interface");
            return false;
        }

        ConnectInterface = PlatformInterface.GetConnectInterface();
        P2PInterface = PlatformInterface.GetP2PInterface();
        LobbyInterface = PlatformInterface.GetLobbyInterface();

        return true;
    }

    internal void ShutdownEOS()
    {
        PlatformInterface?.Release();
        PlatformInterface = null;
        ConnectInterface = null;
        P2PInterface = null;
        LobbyInterface = null;
    }

    internal void ConfigureP2P()
    {
        SetPortRange();
        ConfigureRelayControl();
        
        void SetPortRange()
        {
            SetPortRangeOptions portRangeOptions = new SetPortRangeOptions()
            {
                Port = 7777,
                MaxAdditionalPortsToTry = 99
            };
            
            P2PInterface.SetPortRange(ref portRangeOptions);
        }

        void ConfigureRelayControl()
        {
            SetRelayControlOptions relayControlOptions = new SetRelayControlOptions()
            {
                RelayControl = RelayControl.ForceRelays
            };

            P2PInterface.SetRelayControl(ref relayControlOptions);
        }
    }
}