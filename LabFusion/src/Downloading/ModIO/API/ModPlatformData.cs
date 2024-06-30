using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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