using LabFusion.Utilities;

using MelonLoader.Utils;

using System.Collections;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Determines the username based on the current platform.
/// </summary>
internal static class EOSUsernameDeterminer
{
    public enum Platform
    {
        Steam,
        Rift,
        Quest,
        Unknown
    }

    private const string OculusRiftAppId = "5088709007839657";
    private const string OculusQuestAppId = "4215734068529064";

    private static bool _oculusRequestComplete;
    private static string _oculusUsername;

    public static IEnumerator GetUsernameAsync(Action<string> onComplete)
    {
        var platform = GetPlatform();

        switch (platform)
        {
            case Platform.Steam:
                onComplete?.Invoke(GetSteamUsername());
                yield break;

            case Platform.Rift:
                yield return GetOculusUsernameAsync(OculusRiftAppId, onComplete);
                yield break;

            case Platform.Quest:
                yield return GetOculusUsernameAsync(OculusQuestAppId, onComplete);
                yield break;

            default:
                onComplete?.Invoke("Unknown");
                yield break;
        }
    }

    public static Platform GetPlatform()
    {
        if (PlatformHelper.IsAndroid)
            return Platform.Quest;

        return MelonEnvironment.GameExecutableName switch
        {
            "BONELAB_Steam_Windows64" => Platform.Steam,
            "BONELAB_Oculus_Windows64" => Platform.Rift,
            _ => Platform.Unknown
        };
    }

    private static string GetSteamUsername()
    {
        try
        {
            // Fusion's Steamworks
            if (Steamworks.SteamClient.IsValid)
                Steamworks.SteamClient.Shutdown();
            
            // Game's Steamworks
            if (!Il2CppSteamworks.SteamClient.IsValid)
                Il2CppSteamworks.SteamClient.Init(1592190, false);
            
            return new Il2CppSteamworks.Friend(Il2CppSteamworks.SteamClient.SteamId).Name;
        }
        catch
        {
            return "Unknown";
        }
    }

    private static IEnumerator GetOculusUsernameAsync(string appId, Action<string> onComplete)
    {
        try
        {
            Il2CppOculus.Platform.Core.Initialize(appId);
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("initializing Oculus Platform", ex);
            onComplete?.Invoke("Unknown");
            yield break;
        }

        _oculusRequestComplete = false;
        _oculusUsername = "Unknown";

        var request = Il2CppOculus.Platform.Users.GetLoggedInUser();
        request.OnComplete((Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User>.Callback)OnOculusUserReceived);

        while (!_oculusRequestComplete)
            yield return null;

        onComplete?.Invoke(_oculusUsername);
    }

    private static void OnOculusUserReceived(Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User> msg)
    {
        _oculusUsername = (!msg.IsError && msg.Data != null)
            ? msg.Data.OculusID ?? "Unknown"
            : "Unknown";

        _oculusRequestComplete = true;
    }

    private static readonly string[] Prefixes =
    {
        "Super", "Mega", "Ultra", "Hyper", "Turbo", "Alpha", "Beta", "Gamma",
        "Dark", "Light", "Swift", "Bold", "Wise", "Brave", "Cool", "Epic"
    };

    private static readonly string[] Suffixes =
    {
        "Master", "Lord", "King", "Queen", "Warrior", "Sage", "Knight", "Mage",
        "Hunter", "Rider", "Walker", "Seeker", "Keeper", "Slayer", "Blade", "Storm"
    };

    public static string GetRandomUsername(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 2)
            return "Unknown";

        int prefixIndex = System.Math.Abs(input[0] + input[1]) % Prefixes.Length;
        int suffixIndex = System.Math.Abs(input[^1] + input[^2]) % Suffixes.Length;

        return $"{Prefixes[prefixIndex]}{Suffixes[suffixIndex]}";
    }
}