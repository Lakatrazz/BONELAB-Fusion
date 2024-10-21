using Il2CppSLZ.Marrow.Forklift.Model;

using LabFusion.Utilities;

using MelonLoader;

using Newtonsoft.Json.Linq;

using System.Collections;

namespace LabFusion.Downloading.ModIO;

public static class ModIOManager
{
    public static ModIOModTarget GetTargetFromListing(ModListing listing)
    {
        if (listing == null)
        {
            return null;
        }

        foreach (var target in listing.Targets.Values)
        {
            var modIOTarget = target.TryCast<ModIOModTarget>();

            if (modIOTarget != null)
            {
                return modIOTarget;
            }
        }

        return null;
    }

    public static void GetMod(int modId, ModCallback modCallback)
    {
        var url = $"{ModIOSettings.GameApiPath}{modId}";

        ModIOSettings.LoadToken(OnTokenLoaded);

        void OnTokenLoaded(string token)
        {
            // If the token is null, it likely didn't load
            if (string.IsNullOrWhiteSpace(token))
            {
                modCallback?.Invoke(ModCallbackInfo.FailedCallback);

                return;
            }

            MelonCoroutines.Start(CoGetMod(url, token, modCallback));
        }
    }

    private static IEnumerator CoGetMod(string url, string token, ModCallback modCallback)
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        // Read the mod json
        Task<Stream> streamTask;

        // Handle any errors by running the callback as a failure
        try
        {
            streamTask = client.GetStreamAsync(url);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("getting stream from HttpClient", e);

            modCallback?.Invoke(ModCallbackInfo.FailedCallback);
            yield break;
        }

        // Wait for completion
        while (!streamTask.IsCompleted)
        {
            yield return null;
        }

        // Check for failure
        if (!streamTask.IsCompletedSuccessfully)
        {
            modCallback?.Invoke(ModCallbackInfo.FailedCallback);

            yield break;
        }

        Task<string> jsonTask;

        // Handle any errors by running the callback as a failure
        try
        {
            jsonTask = new StreamReader(streamTask.Result).ReadToEndAsync();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("reading mod.io mod stream", e);

            modCallback?.Invoke(ModCallbackInfo.FailedCallback);

            yield break;
        }

        // Wait for completion
        while (!jsonTask.IsCompleted)
        {
            yield return null;
        }

        // Check for failure
        if (!jsonTask.IsCompletedSuccessfully)
        {
            modCallback?.Invoke(ModCallbackInfo.FailedCallback);

            yield break;
        }

        // Convert to ModData
        var jObject = JObject.Parse(jsonTask.Result);

        var modData = new ModData(jObject);
        var modCallbackInfo = new ModCallbackInfo()
        {
            data = modData,
            result = ModResult.SUCCEEDED,
        };

        modCallback?.Invoke(modCallbackInfo);
    }

    public static string GetActivePlatform()
    {
        if (PlatformHelper.IsAndroid)
        {
            return "android";
        }
        else
        {
            return "windows";
        }
    }

    public static ModPlatformData? GetValidPlatform(ModData mod)
    {
        string activePlatform = GetActivePlatform();

        foreach (var platform in mod.Platforms)
        {
            if (platform.Platform != activePlatform)
            {
                continue;
            }

            return platform;
        }

        return null;
    }
}
