using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSPlatform : EOSInterface
{
    private const string ProductName = "Fusion";
    private const string ProductVersion = "0.0.1";
    private const string ProductId = "29e074d5b4724f3bb01f26b7e33d2582";
    private const string ClientId = "xyza78915hKqxe2TNTavpq2sxBDvJ9AH";
    private const string ClientSecret  = "SWDxYlWWsEgvmD0o3qAm2RMZoSZzOfYo5yvX/uikH94";
    private const string SandboxId = "26f32d66d87f4dfeb4a7449b776a41f1";
    private const string DeploymentId = "f3fdf691aa6c4004abdb1e19665c1429";
    private const PlatformFlags Flags = PlatformFlags.DisableOverlay | PlatformFlags.DisableSocialOverlay;
    private const float TickInterval = 1f / 20f;

    internal PlatformInterface PlatformInterface;
    
    internal override IEnumerator InitializeAsync(Action<bool> onComplete)
    {
        if (PlatformHelper.IsAndroid)
            EOSJNI.Initialize();
        
        if (!InitializePlatform())
        {
            onComplete?.Invoke(false);
            yield break;
        }
        
        if (!CreatePlatform(out PlatformInterface))
        {
            onComplete?.Invoke(false);
            yield break;
        }
        
#if DEBUG
        LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Info);
        LoggingInterface.SetCallback((ref LogMessage message) => FusionLogger.Log($"EOS -> [{message.Category}] [{message.Level.ToString()}] {message.Message}"));
#endif
        
        InitializeTicker();
        
        onComplete?.Invoke(true);
        
        yield return null;
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
            FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
            return false;
        }

        return true;
    }

    private bool CreatePlatform(out PlatformInterface platformInterface)
    {
        var options = new Options
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
        
        var platform = PlatformInterface.Create(ref options);
        if (platform == null)
        {
            FusionLogger.Error("Failed to create EOS Platform");
            platformInterface = null;
            return false;
        }
        
        platformInterface = platform;
        
        return true;
    }

    private void InitializeTicker()
    {
        MelonLoader.MelonCoroutines.Start(Tick());

        IEnumerator Tick()
        {
            float elapsed = 0f;

            while (PlatformInterface != null)
            {
                elapsed += TimeReferences.UnscaledDeltaTime;
                if (elapsed >= TickInterval)
                {
                    elapsed -= TickInterval;

                    try
                    {
                        PlatformInterface.Tick();
                    }
                    catch (Exception ex)
                    {
                        FusionLogger.LogException("ticking EOS platform", ex);
                    }
                }

                yield return null;
            }
            
#if DEBUG
            FusionLogger.Log("EOS Platform ticker stopped");
#endif
            
            yield return null;
        }
    }
    
    internal override void Shutdown()
    {
        PlatformInterface?.Release();
        PlatformInterface = null;
    }
}