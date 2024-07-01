using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModData
{
    public int Id { get; }

    public IReadOnlyList<ModPlatformData> Platforms { get; }

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
    }
}
