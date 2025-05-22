using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlatformInfo : IEquatable<PlatformInfo>
{
    [JsonPropertyName("platformId")]
    public ulong PlatformId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    public PlatformInfo(ulong platformId)
    {
        PlatformId = platformId;

        if (NetworkLayerManager.HasLayer)
        {
            Platform = NetworkLayerManager.Layer.Platform;
        }
    }

    public bool Equals(PlatformInfo other)
    {
        return other is not null && PlatformId == other.PlatformId && Platform == other.Platform;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as PlatformInfo);
    }

    public override int GetHashCode() => PlatformId.GetHashCode() ^ Platform.GetHashCode();

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
