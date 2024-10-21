using LabFusion.Utilities;
using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

public delegate void ModCallback(ModCallbackInfo info);

public struct ModCallbackInfo
{
    public static readonly ModCallbackInfo FailedCallback = new()
    {
        data = default,
        result = ModResult.FAILED,
    };

    public ModData data;
    public ModResult result;
}

[Serializable]
public readonly struct ModData
{
    public int Id { get; }

    public IReadOnlyList<ModPlatformData> Platforms { get; }

    public string ThumbnailUrl { get; }

    public ModData(JToken token)
    {
        Id = token.Value<int>("id");

        List<ModPlatformData> modPlatformList = new();

        var platforms = token["platforms"].ToArray();
        foreach (var platform in platforms)
        {
            modPlatformList.Add(new ModPlatformData(platform));
        }

        Platforms = modPlatformList;

        ThumbnailUrl = token["logo"]["thumb_640x360"].ToString();
    }
}
