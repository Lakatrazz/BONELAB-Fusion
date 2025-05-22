using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class GameInfo : IEquatable<GameInfo>
{
    [JsonPropertyName("game")]
    public string Game { get; set; }

    public bool Equals(GameInfo other)
    {
        return other is not null && Game == other.Game;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as GameInfo);
    }

    public override int GetHashCode() => Game.GetHashCode();

    public static bool operator ==(GameInfo x, GameInfo y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public static bool operator !=(GameInfo x, GameInfo y)
    {
        if (x is null)
        {
            return y is not null;
        }

        return !x.Equals(y);
    }
}
