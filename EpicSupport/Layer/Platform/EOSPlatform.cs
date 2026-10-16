using Epic.OnlineServices;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;

namespace MarrowFusion.Epic;

internal class EOSPlatform : EOSInterface
{
    internal const string ProductName = "Marrow Fusion";
    internal const string ProductVersion = "0.0.1";
    internal const string ProductId = "29e074d5b4724f3bb01f26b7e33d2582";
    internal const string ClientId = "xyza78915hKqxe2TNTavpq2sxBDvJ9AH";
    internal const string ClientSecret  = "SWDxYlWWsEgvmD0o3qAm2RMZoSZzOfYo5yvX/uikH94";
    internal const string SandboxId = "26f32d66d87f4dfeb4a7449b776a41f1";
    internal const string DeploymentId = "f3fdf691aa6c4004abdb1e19665c1429";
    private const PlatformFlags Flags = PlatformFlags.DisableOverlay | PlatformFlags.DisableSocialOverlay;
    
    internal PlatformInterface PlatformInterface;

    internal override Task<bool> InitializeAsync()
    {
        if (!InitializePlatform())
            return Task.FromResult(false);
        
        if (!CreatePlatform(out PlatformInterface))
            return Task.FromResult(false);
        
#if DEBUG
        LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
        LoggingInterface.SetCallback((ref LogMessage message) => EpicModule.Logger.Log($"[{message.Category}] [{message.Level.ToString()}] {message.Message}"));
#endif
        
        return Task.FromResult(true);
    }
    
    private bool InitializePlatform()
    {
        var initializeOptions = new InitializeOptions
        {
            ProductName = ProductName,
            ProductVersion = ProductVersion
        };
        
        var initializeResult = PlatformInterface.Initialize(ref initializeOptions);
        if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
        {
            EpicModule.Logger.Error($"Failed to initialize EOS Platform: {initializeResult}");
            return false;
        }

        return true;
    }
    
    private bool CreatePlatform(out PlatformInterface platformInterface)
    {
        var platformOptions = new Options
        {
            ProductId = ProductId,
            SandboxId = SandboxId,
            DeploymentId = DeploymentId,
            ClientCredentials = new ClientCredentials
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            },
            Flags = Flags
        };
        
        var platform = PlatformInterface.Create(ref platformOptions);
        if (platform == null)
        {
            EpicModule.Logger.Error("Failed to create EOS Platform");
            platformInterface = null;
            return false;
        }
        
        platformInterface = platform;
        
        return true;
    }

    internal override void Tick()
    {
        PlatformInterface?.Tick();
    }
}