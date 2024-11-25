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
    public string NameId { get; }

    public int Id { get; }

    public int MaturityOption { get; }

    public IReadOnlyList<ModPlatformData> Platforms { get; }

    public string ThumbnailUrl { get; }

    public bool Mature => MaturityOption > 0;

    public ModData(JToken token)
    {
        NameId = token.Value<string>("name_id");

        Id = token.Value<int>("id");

        MaturityOption = token.Value<int>("maturity_option");

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
