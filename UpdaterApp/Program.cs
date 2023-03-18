// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/UpdaterApp/Program.cs

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace UpdaterApp
{
    internal class Program {
        private static readonly string _releasesApi = "https://api.github.com/repos/Lakatrazz/BONELAB-Fusion/releases";
        
        public const string ModName = "LabFusion";
        public const string PluginName = "LabFusionUpdater";
        public const string AssemblyExtension = ".dll";

        static int Main(string[] args)
        {
            try
            {
                Version localVersion = new Version(args[0]);
                string fusionAssemblyPath = args[1];
                string fusionUpdaterAssemblyPath = args[2];
                bool updatePlugin = args[3] == "true";

                using (HttpClient client = new HttpClient())
                {
                    // Web request for getting all versions of LabFusion from github API
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_releasesApi);
                    request.Accept = "application/vnd.github.v3.raw";
                    request.UserAgent = PluginName;
                    WebResponse response = request.GetResponse();

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        // Deserialize the response into json
                        string fileContent = reader.ReadToEnd();
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        dynamic releases = jsonSerializer.Deserialize<dynamic>(fileContent);

                        // Find the release info for the latest version
                        Version latestVersion = new Version(0, 0, 0);
                        dynamic latestRelease = null;
                        foreach (var release in releases)
                        {
                            Version version = new Version(((string)release["tag_name"]).Replace("v", ""));
                            if (version >= latestVersion)
                            {
                                latestVersion = version;
                                latestRelease = release;
                            }
                        }

                        Console.WriteLine($"Latest version of {ModName} is {latestVersion}.");

                        if (!updatePlugin)
                        {
                            if (latestVersion > localVersion)
                            {
                                Console.WriteLine("Downloading latest version...");
                                bool downloadedMod = false;
                                foreach (var asset in latestRelease["assets"])
                                {
                                    if (asset["name"] == $"{ModName}{AssemblyExtension}")
                                    {
                                        string downloadUrl = asset["browser_download_url"];
                                        using (HttpClient downloadClient = new HttpClient())
                                        {
                                            // Download the latest version of LabFusion.dll and save it to the mods folder
                                            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
                                            downloadRequest.Accept = "application/vnd.github.v3.raw";
                                            downloadRequest.UserAgent = PluginName;
                                            WebResponse downloadResponse = downloadRequest.GetResponse();
                                            using (Stream downloadStream = downloadResponse.GetResponseStream())
                                            {
                                                using (FileStream fileStream = new FileStream(fusionAssemblyPath, FileMode.Create, FileAccess.Write))
                                                {
                                                    downloadStream.CopyTo(fileStream);
                                                    downloadedMod = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (downloadedMod)
                                    return (int)ExitCode.Success;
                                return (int)ExitCode.Error;
                            }
                            else
                            {
                                return (int)ExitCode.UpToDate;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Downloading latest version...");
                            foreach (var asset in latestRelease["assets"])
                            {
                                if (asset["name"] == $"{PluginName}{AssemblyExtension}")
                                {
                                    string downloadUrl = asset["browser_download_url"];
                                    using (HttpClient downloadClient = new HttpClient())
                                    {
                                        // Download the latest version of LabFusionUpdater.dll and save it to the plugins folder
                                        HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
                                        downloadRequest.Accept = "application/vnd.github.v3.raw";
                                        downloadRequest.UserAgent = PluginName;
                                        WebResponse downloadResponse = downloadRequest.GetResponse();
                                        using (Stream downloadStream = downloadResponse.GetResponseStream())
                                        {
                                            using (FileStream fileStream = new FileStream(fusionUpdaterAssemblyPath, FileMode.Create, FileAccess.Write))
                                            {
                                                downloadStream.CopyTo(fileStream);
                                                Console.WriteLine($"Successfully updated {PluginName}{AssemblyExtension}!");
                                                return (int)ExitCode.Success;
                                            }
                                        }
                                    }
                                }
                            }

                            return (int)ExitCode.Error;
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Exception caught while running {ModName} updater!");
                Console.WriteLine(e.ToString());

                return (int)ExitCode.Error;
            }
        }
    }

    enum ExitCode {
        Success = 0,
        UpToDate = 1,
        Error = 2
    }
}