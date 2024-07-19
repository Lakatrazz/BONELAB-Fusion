using BoneLib;

using Il2CppSLZ.Marrow.Forklift.Model;

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

    public static void GetMod(int modId, Action<ModData> modCallback)
    {
        var url = $"{ModIOSettings.GameApiPath}{modId}";

        ModIOSettings.LoadToken(OnTokenLoaded);

        void OnTokenLoaded(string token)
        {
            MelonCoroutines.Start(CoGetMod(url, token, modCallback));
        }
    }

    private static IEnumerator CoGetMod(string url, string token, Action<ModData> modCallback)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        // Read the mod json
        var streamTask = client.GetStreamAsync(url);

        while (!streamTask.IsCompleted)
        {
            yield return null;
        }

        var jsonTask = new StreamReader(streamTask.Result).ReadToEndAsync();

        while (!jsonTask.IsCompleted)
        {
            yield return null;
        }

        // Convert to ModData
        var jObject = JObject.Parse(jsonTask.Result);

        var modData = new ModData(jObject);
        modCallback?.Invoke(modData);
    }

    public static string GetActivePlatform()
    {
        if (HelperMethods.IsAndroid())
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
