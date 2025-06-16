using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModPlatformData
{
    public string Platform { get; }

    public int ModFileLive { get; }

    public ModPlatformData(JToken token)
    {
        Platform = token.Value<string>("platform");
        ModFileLive = token.Value<int>("modfile_live");
    }
}