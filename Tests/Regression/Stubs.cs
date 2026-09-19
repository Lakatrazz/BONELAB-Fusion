global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Net.Http;

namespace LabFusion.Utilities
{
    public static class FusionLogger
    {
        public static void Warn(string message) => Console.WriteLine(message);
        public static void Log(string message) { }
        public static void Error(string message) { }
        public static void LogException(string context, Exception error) => Console.WriteLine($"{context}: {error.GetType().Name}");
    }
}

namespace UnityEngine
{
    public static class Application
    {
        public static string persistentDataPath;
    }
}

namespace MelonLoader
{
    public static class MelonCoroutines
    {
        public static void Start(System.Collections.IEnumerator routine)
        {
            while (routine.MoveNext()) System.Threading.Thread.Sleep(1);
        }
    }
}

namespace Il2CppSLZ.Marrow.Warehouse { public class Pallet { } }

namespace Il2CppSLZ.Marrow.Forklift.Model
{
    public class StringModTargetListingDictionary : Dictionary<string, ModIOModTarget> { }
    public class ModIOModTarget
    {
        public int GameId, ModId, ModfileId;
    }
    public class ModListing
    {
        public object Author, Barcode, Description, Repository;
        public StringModTargetListingDictionary Targets;
    }
}

namespace LabFusion.Downloading.ModIO
{
    public static class ModIOSettings
    {
        public const int GameID = 3809;
        public static string FormatDownloadPath(int mod, int file) => throw new NotSupportedException();
        public static void LoadToken(Action<string> callback) => throw new NotSupportedException();
    }
    public static class ModIOManager
    {
        public static string GetActivePlatform() => "windows";
        public static void GetMod(int id, Action<ModCallbackInfo> callback) => throw new NotSupportedException();
        public static ModPlatformData? GetValidPlatform(ModData data) => null;
    }
    public struct ModPlatformData { public int ModFileLive; }
    public class ModData { public bool Mature; public string NameID; public int ID; }
    public struct ModCallbackInfo { public LabFusion.Downloading.ModResult Result; public ModData Data; }
}

namespace LabFusion.Downloading
{
    public static class ModForklift
    {
        public static int Scheduled;
        public static void UpdateForklift() { }
        public struct PalletShipment
        {
            public string palletPath;
            public Il2CppSLZ.Marrow.Forklift.Model.ModListing modListing;
            public DownloadCallback callback;
        }
        public static void SchedulePalletLoad(PalletShipment shipment) => Scheduled++;
    }
}

namespace LabFusion.Data { }
namespace LabFusion.Preferences
{
    public static class CommonPreferences { public static bool ShowMatureMods; }
}
namespace LabFusion.Safety
{
    public static class ModBlacklist { public static bool IsBlacklisted(string id) => false; }
    public static class GlobalModBlacklistManager
    {
        public static bool IsNameIDBlacklisted(string id) => false;
        public static bool IsModIDBlacklisted(int id) => false;
    }
}
namespace LabFusionUpdater
{
    public class TestLogger
    {
        public void Msg(string text) { }
        public void Error(string text) { }
    }
    public class FusionUpdaterPlugin
    {
        public const string ModName = "LabFusion", FileExtension = ".dll";
        public static string ModAssemblyPath;
        public static TestLogger Logger = new();
        public static FusionUpdaterPlugin Instance = new();
        public TestLogger LoggerInstance = new();
    }
}
