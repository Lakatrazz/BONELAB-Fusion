using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModFileData
{
    public long FileSize { get; }

    public ModFileData(JToken token)
    {
        FileSize = token.Value<long>("filesize");
    }
}