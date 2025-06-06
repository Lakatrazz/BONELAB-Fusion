using Newtonsoft.Json.Linq;

namespace LabFusion.Downloading.ModIO;

public delegate void ModCallback(ModCallbackInfo info);

public struct ModCallbackInfo
{
    public static readonly ModCallbackInfo FailedCallback = new()
    {
        Data = default,
        Result = ModResult.FAILED,
    };

    public ModData Data;

    public ModResult Result;
}

[Serializable]
public readonly struct ModData
{
    public string NameID { get; }

    public int ID { get; }

    public int MaturityOption { get; }

    public IReadOnlyList<ModPlatformData> Platforms { get; }

    public string ThumbnailUrl { get; }

    public bool Mature => MaturityOption > 0;

    public ModData(JToken token)
    {
        NameID = token.Value<string>("name_id");

        ID = token.Value<int>("id");

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
