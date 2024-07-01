using Il2CppSLZ.Marrow.Forklift;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading.ModIO;

public static class ModIOSettings
{
    public const string ApiPath = "https://api.mod.io/v1/games/";
    public static string GameApiPath => $"{ApiPath}{GameId}/mods/";
    public static int GameId => ModDownloader.ModIOManager.ModIOGameId;

    private static string _loadedToken = null;
    public static string LoadedToken => _loadedToken;

    public static async Task<string> LoadTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_loadedToken))
        {
            return _loadedToken;
        }

        var settingsPath = ModDownloader.ModSettingsPath;

        using var stream = new FileStream(settingsPath, FileMode.Open);

        var reader = new StreamReader(stream);

        var settingsContents = await reader.ReadToEndAsync();

        JObject settingsJson = JObject.Parse(settingsContents);

        var token = settingsJson["mod.io.access_token"].ToString();

        _loadedToken = token;

        return token;
    }
}