using BoneLib;

using Il2CppSLZ.Marrow.Forklift.Model;

using Newtonsoft.Json.Linq;

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

    public static async Task<ModData> GetMod(int modId)
    {
        var url = $"{ModIOSettings.GameApiPath}{modId}";

        // Get token for authorization
        string token = await ModIOSettings.LoadTokenAsync();

        HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        // Read the mod json
        var stream = await client.GetStreamAsync(url);

        var json = await new StreamReader(stream).ReadToEndAsync();

        // Convert to ModData
        var jObject = JObject.Parse(json);
        return new ModData(jObject);
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
