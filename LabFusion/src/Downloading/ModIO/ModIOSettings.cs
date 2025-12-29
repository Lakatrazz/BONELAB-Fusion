using Il2CppSLZ.Marrow.Forklift;

using LabFusion.Utilities;

using MelonLoader;

using Newtonsoft.Json.Linq;

using System.Collections;

namespace LabFusion.Downloading.ModIO;

public static class ModIOSettings
{
    public const string ApiPath = "https://api.mod.io/v1/games/";
    public const int GameID = 3809; // BONELAB GameID
    public static string GameApiPath => $"{ApiPath}{GameID}/mods/";

    private static string _loadedToken = null;
    public static string LoadedToken => _loadedToken;

    private static bool _isLoadingToken = false;
    public static bool IsLoadingToken => _isLoadingToken;

    private static bool _loggedMissingToken = false;

    private static Action<string> _tokenLoadCallback = null;

    public static string FormatFilePath(int modId, int fileId)
    {
        return $"{GameApiPath}{modId}/files/{fileId}";
    }

    public static string FormatDownloadPath(int modId, int fileId)
    {
        return $"{FormatFilePath(modId, fileId)}/download";
    }

    public static void LoadToken(Action<string> loadCallback)
    {
        if (!string.IsNullOrWhiteSpace(LoadedToken))
        {
            loadCallback?.Invoke(LoadedToken);
            return;
        }

        _tokenLoadCallback += loadCallback;

        if (IsLoadingToken)
        {
            return;
        }

        MelonCoroutines.Start(CoLoadToken());
    }

    private static IEnumerator CoLoadToken()
    {
        // Start loading
        _isLoadingToken = true;

        var settingsPath = ModDownloader.ModSettingsPath;

        if (!File.Exists(settingsPath))
        {
            if (!_loggedMissingToken)
            {
                FusionLogger.Error("mod.io token is missing! Please set it in the mods menu!");
                _loggedMissingToken = true;
            }

            EndLoadToken(null);

            yield break;
        }

        using var stream = new FileStream(settingsPath, FileMode.Open);

        var reader = new StreamReader(stream);

        var settingsTask = reader.ReadToEndAsync();

        while (!settingsTask.IsCompleted)
        {
            yield return null;
        }

        if (!settingsTask.IsCompletedSuccessfully)
        {
            FusionLogger.Error("Failed reading mod.io token from settings!");

            EndLoadToken(null);

            yield break;
        }

        JObject settingsJson = JObject.Parse(settingsTask.Result);

        var token = settingsJson["mod.io.access_token"].ToString();

        EndLoadToken(token);
    }

    private static void EndLoadToken(string token)
    {
        _loadedToken = token;

        try
        {
            _tokenLoadCallback?.Invoke(token);
            _tokenLoadCallback = null;
        }
        catch (Exception e)
        {
            FusionLogger.LogException("invoking token load callback", e);
        }

        // Make sure that this is set AFTER invoking the callback
        // Otherwise, not sure why, a stack overflow can be caused!
        _isLoadingToken = false;
    }
}