using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices;

using LabFusion.Utilities;

using System.Collections;

using MelonLoader;

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
    
    private static IEnumerator Ticker()
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
    
    internal static IEnumerator Initialize(System.Action<bool> onComplete)
    {
        LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
        LoggingInterface.SetCallback(((ref LogMessage message) =>
        {
            FusionLogger.Log("EOS -> " + message.Message);
        }));
        
        if (!InitializeInterfaces())
        {
            onComplete?.Invoke(false);
            yield break;
        }
        
        MelonCoroutines.Start(Ticker());
    }

    private static bool InitializeInterfaces()
    {
        var initializeOptions = new InitializeOptions();

        initializeOptions.ProductName = AuthCredentials.ProductName;
        initializeOptions.ProductVersion = AuthCredentials.ProductVersion;

        Result initializeResult = PlatformInterface.Initialize(ref initializeOptions);
        
        if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
        {
            FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
            return false;
        }
        
        Options options = new Options()
        {
            ProductId = AuthCredentials.ProductId,
            SandboxId = AuthCredentials.SandboxId,
            DeploymentId = AuthCredentials.DeploymentId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = AuthCredentials.ClientId,
                ClientSecret = AuthCredentials.ClientSecret
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
}