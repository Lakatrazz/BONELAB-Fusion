using LabFusion.Utilities;

using MelonLoader.Utils;

using System.Collections;

namespace LabFusion.Network.EpicGames;

internal static class EOSUsernameDeterminer
{
    internal enum Platform
    {
        Steam,
        Rift,
        Quest,
        Unknown
    }

    private static bool _oculusDone;
    private static string _oculusUsername;

    internal static Platform GetPlatform()
    {
        if (PlatformHelper.IsAndroid)
            return Platform.Quest;

        if (MelonEnvironment.GameExecutableName == "BONELAB_Steam_Windows64")
            return Platform.Steam;
        else if (MelonEnvironment.GameExecutableName == "BONELAB_Oculus_Windows64")
            return Platform.Rift;
        else
            return Platform.Unknown;
    }

    internal static IEnumerator GetUsernameAsync(System.Action<string> onComplete)
    {
        switch (GetPlatform())
        {
            case Platform.Steam:
                onComplete.Invoke(new Il2CppSteamworks.Friend(Il2CppSteamworks.SteamClient.SteamId).Name);
                yield break;

            case Platform.Rift:
                yield return GetUsernameOculus("5088709007839657", onComplete);
                yield break;

            case Platform.Quest:
                yield return GetUsernameOculus("4215734068529064", onComplete);
                yield break;

            default:
                onComplete.Invoke("Unknown");
                yield break;
        }
    }

    private static IEnumerator GetUsernameOculus(string appId, System.Action<string> onComplete)
    {
        Il2CppOculus.Platform.Core.Initialize(appId);

        _oculusDone = false;
        _oculusUsername = "Unknown";

        var request = Il2CppOculus.Platform.Users.GetLoggedInUser();
        request.OnComplete((Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User>.Callback)OnOculusUserReceived);

        while (!_oculusDone)
            yield return null;

        onComplete.Invoke(_oculusUsername);
    }

    private static void OnOculusUserReceived(Il2CppOculus.Platform.Message<Il2CppOculus.Platform.Models.User> msg)
    {
        if (!msg.IsError && msg.Data != null)
            _oculusUsername = msg.Data.OculusID;
        else
            _oculusUsername = "Unknown";

        _oculusDone = true;
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

    internal static string GetRandomUsername(string input)
    {
        int prefixIndex = System.Math.Abs(input[0] + input[1]) % Prefixes.Length;
        int suffixIndex = System.Math.Abs(input[^1] + input[^2]) % Suffixes.Length;

        return $"{Prefixes[prefixIndex]}{Suffixes[suffixIndex]}";
    }
}