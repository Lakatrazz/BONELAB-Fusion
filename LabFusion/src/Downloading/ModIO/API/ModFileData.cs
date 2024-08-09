using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

public delegate void ModFileCallback(ModFileCallbackInfo info);

public struct ModFileCallbackInfo
{
    public static readonly ModFileCallbackInfo FailedCallback = new()
    {
        data = default,
        result = ModResult.FAILED,
    };

    public ModFileData data;
    public ModResult result;
}

[Serializable]
public readonly struct ModFileData
{
    public long FileSize { get; }

    public ModFileData(JToken token)
    {
        FileSize = token.Value<long>("filesize");
    }
}
