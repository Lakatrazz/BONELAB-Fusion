using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using LabFusion.Data;
using LabFusion.Utilities;
using MelonLoader;
using Newtonsoft.Json;
using SLZ.ModIO.ApiModels;

namespace LabFusion.Network
{
    public class SceneLoadData : IFusionSerializable
    {
        public string levelBarcode;
        public string loadBarcode;

        public static int GetSize(string barcode, string loadBarcode)
        {
            return barcode.Length + loadBarcode.Length;
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(levelBarcode);
            writer.Write(loadBarcode);
        }

        public void Deserialize(FusionReader reader)
        {
            levelBarcode = reader.ReadString();
            loadBarcode = reader.ReadString();
        }

        public static SceneLoadData Create(string levelBarcode, string loadBarcode)
        {
            return new SceneLoadData()
            {
                levelBarcode = levelBarcode,
                loadBarcode = loadBarcode
            };
        }
    }

    public class SceneLoadMessage : FusionMessageHandler
    {
        private static readonly HttpClient httpClient;

        static SceneLoadMessage()
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            httpClient.DefaultRequestHeaders.Add("User-Agent", "LabFusion-ModDownloader");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public override byte? Tag => NativeMessageTag.SceneLoad;

        public override async void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.IsServer && !isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<SceneLoadData>();

                MelonLogger.Msg($"Received level load for {data.levelBarcode}!");

                try
                {
                    string downloadUrl = await GetModDownloadUrl(data.levelBarcode);
                    await DownloadMod(downloadUrl, data.levelBarcode);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error downloading mod: {ex.Message}");
                    MelonLogger.Error($"Failed to download level: {data.levelBarcode}");
                }

                FusionSceneManager.SetTargetScene(data.levelBarcode, data.loadBarcode);
            }
        }

        private async Task<string> GetModDownloadUrl(string levelBarcode)
        {
            //Idk if this is the correct way to use the api, their api is so fucking weird so this is just an example for later or if someone wants to change it
            // Replace with actual call to BONELAB API to get mod information
            var mod = await ModAPI.GetModByBarcode(levelBarcode);
            if (mod.Modfile == null)
            {
                throw new Exception("Modfile is missing");
            }

            if (mod.Modfile.Download == null)
            {
                throw new Exception("Download URL is missing");
            }

            return mod.Modfile.Download.url;
        }

        private async Task DownloadMod(string url, string levelBarcode)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{levelBarcode}.mod");
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }

                MelonLogger.Msg($"Successfully downloaded mod to {tempFilePath}");
                // Here, you can add logic to process the downloaded file if needed
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading mod from {url}: {ex.Message}");
            }
        }
    }

    // Placeholder class to represent the ModAPI
    public static class ModAPI
    {
        public static async Task<ModObject> GetModByBarcode(string barcode)
        {
            // Placeholder: Implement actual logic to call BONELAB API and retrieve mod information by barcode
            await Task.Delay(100); // Simulate async API call
            return new ModObject
            {
                // Initialize with sample data
                Modfile = new ModfileObject
                {
                    Download = new DownloadObject
                    {
                        url = "https://example.com/download"
                    }
                }
            };
        }
    }

    // Placeholder classes to represent API models
    public class ModObject
    {
        public ModfileObject Modfile { get; set; }
    }

    public class ModfileObject
    {
        public DownloadObject Download { get; set; }
    }

    public class DownloadObject
    {
        public string url { get; set; }
    }
}
