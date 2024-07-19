using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModPlatformData
{
    public string Platform { get; }

    public int ModfileLive { get; }

    public ModPlatformData(JToken token)
    {
        Platform = token.Value<string>("platform");
        ModfileLive = token.Value<int>("modfile_live");
    }
}