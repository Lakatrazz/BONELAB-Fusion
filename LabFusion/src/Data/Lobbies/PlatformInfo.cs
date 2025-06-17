using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlatformInfo : IEquatable<PlatformInfo>
{
    [JsonPropertyName("platformID")]
    public ulong PlatformID { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    public PlatformInfo(ulong platformID)
    {
        PlatformID = platformID;

        if (NetworkLayerManager.HasLayer)
        {
            Platform = NetworkLayerManager.Layer.Platform;
        }
    }

    public bool Equals(PlatformInfo other)
    {
        return other is not null && PlatformID == other.PlatformID && Platform == other.Platform;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as PlatformInfo);
    }

    public override int GetHashCode() => PlatformID.GetHashCode() ^ Platform.GetHashCode();

    public static bool operator ==(PlatformInfo x, PlatformInfo y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public static bool operator !=(PlatformInfo x, PlatformInfo y)
    {
        if (x is null)
        {
            return y is not null;
        }

        return !x.Equals(y);
    }
}
