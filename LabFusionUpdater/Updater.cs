// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLibUpdater/Updater.cs

using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LabFusionUpdater;

public static class Updater
{
    public enum ExitResult
    {
        Success = 0,
        UpToDate = 1,
        Error = 2
    }

    public const string ReleasesApi = "https://api.github.com/repos/Lakatrazz/BONELAB-Fusion/releases";

    public const string ModName = "LabFusion";
    public const string AssemblyExtension = ".dll";

    public const string ModFileName = ModName + AssemblyExtension;

    public const string AcceptHeaderName = "Accept";
    public const string AcceptHeaderValue = "application/vnd.github.v3.raw";

    public const string UserAgentHeaderName = "User-Agent";
    public const string UserAgentHeaderValue = "LabFusionUpdater";

    public static void UpdateMod()
    {
        // Check for local version of mod and read version if it exists
        var localVersion = new Version(0, 0, 0);

        if (File.Exists(FusionUpdaterPlugin.ModAssemblyPath))
        {
            AssemblyName localAssemblyInfo = AssemblyName.GetAssemblyName(FusionUpdaterPlugin.ModAssemblyPath);

            localVersion = new Version(localAssemblyInfo.Version.Major, localAssemblyInfo.Version.Minor, localAssemblyInfo.Version.Build); // Remaking the object so there's no 4th number

            FusionUpdaterPlugin.Logger.Msg($"{FusionUpdaterPlugin.ModName}{FusionUpdaterPlugin.FileExtension} found in Mods folder. Version: {localVersion}");
        }

        try
        {
            var result = DownloadMod(localVersion, FusionUpdaterPlugin.ModAssemblyPath);

            switch (result)
            {
                case ExitResult.Success:
                    FusionUpdaterPlugin.Instance.LoggerInstance.Msg($"{FusionUpdaterPlugin.ModName}{FusionUpdaterPlugin.FileExtension} updated successfully!");
                    break;
                case ExitResult.UpToDate:
                    FusionUpdaterPlugin.Instance.LoggerInstance.Msg($"{FusionUpdaterPlugin.ModName}{FusionUpdaterPlugin.FileExtension} is already up to date.");
                    break;
                case ExitResult.Error:
                    FusionUpdaterPlugin.Instance.LoggerInstance.Error($"{FusionUpdaterPlugin.ModName}{FusionUpdaterPlugin.FileExtension} failed to update!");
                    break;
            }
        }
        catch (Exception e)
        {
            FusionUpdaterPlugin.Logger.Error($"Exception caught while running {FusionUpdaterPlugin.ModName} updater!");
            FusionUpdaterPlugin.Logger.Error(e.ToString());
        }
    }

    private static ExitResult DownloadMod(Version currentVersion, string modAssemblyPath)
    {
        // Setup the HttpClient and request for getting information from github
        using var client = new HttpClient();

        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using HttpClient request = new(handler);

        request.DefaultRequestHeaders.Add(AcceptHeaderName, AcceptHeaderValue);
        request.DefaultRequestHeaders.Add(UserAgentHeaderName, UserAgentHeaderValue);

        try
        {
            var response = request.Send(new HttpRequestMessage(HttpMethod.Get, ReleasesApi));

            using var reader = new StreamReader(response.Content.ReadAsStream(), Encoding.UTF8);

            // Deserialize the response into json
            string fileContent = reader.ReadToEnd();

            JsonNode releases = JsonSerializer.Deserialize<JsonNode>(fileContent);

            // Find the release info for the latest version
            var latestVersion = new Version(0, 0, 0);
            JsonNode latestRelease = null;
            foreach (var release in releases.AsArray())
            {
                var version = new Version(((string)release["tag_name"]).Replace("v", ""));
                if (version >= latestVersion)
                {
                    latestVersion = version;
                    latestRelease = release;
                }
            }

            Console.WriteLine($"Latest version of {ModName} is {latestVersion}.");

            if (latestVersion <= currentVersion)
            {
                return ExitResult.UpToDate;
            }

            Console.WriteLine("Downloading latest version...");

            bool downloadedMod = false;

            foreach (var asset in latestRelease["assets"].AsArray())
            {
                if (asset["name"].ToString() == ModFileName)
                {
                    string downloadUrl = asset["browser_download_url"].ToString();

                    // Send a download request to the http client
                    var downloadResponse = request.Send(new HttpRequestMessage(HttpMethod.Get, downloadUrl));

                    using var fileStream = new FileStream(modAssemblyPath, FileMode.Create, FileAccess.Write);

                    downloadResponse.Content.ReadAsStream().CopyTo(fileStream);
                    downloadedMod = true;
                }
            }

            if (downloadedMod)
            {
                return ExitResult.Success;
            }

            return ExitResult.Error;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception caught while running {ModName} updater!");
            Console.WriteLine(e.ToString());

            return ExitResult.Error;
        }
    }
}