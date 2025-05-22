using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Safety;

[Serializable]
public class ListPlatform : IEquatable<ListPlatform>
{
    [JsonPropertyName("platformId")]
    public ulong PlatformId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    public ListPlatform(ulong platformId)
    {
        PlatformId = platformId;

        if (NetworkLayerManager.HasLayer)
        {
            Platform = NetworkLayerManager.Layer.Platform;
        }
    }

    public bool Equals(ListPlatform other)
    {
        return other is not null && PlatformId == other.PlatformId && Platform == other.Platform;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ListPlatform);
    }

    public override int GetHashCode() => PlatformId.GetHashCode() ^ Platform.GetHashCode();

    public static bool operator ==(ListPlatform x, ListPlatform y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public static bool operator !=(ListPlatform x, ListPlatform y)
    {
        if (x is null)
        {
            return y is not null;
        }

        return !x.Equals(y);
    }
}
