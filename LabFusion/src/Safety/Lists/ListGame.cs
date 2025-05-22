using System.Text.Json.Serialization;

namespace LabFusion.Safety;

[Serializable]
public class ListGame : IEquatable<ListGame>
{
    [JsonPropertyName("game")]
    public string Game { get; set; }

    public bool Equals(ListGame other)
    {
        return other is not null && Game == other.Game;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ListGame);
    }

    public override int GetHashCode() => Game.GetHashCode();

    public static bool operator ==(ListGame x, ListGame y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public static bool operator !=(ListGame x, ListGame y)
    {
        if (x is null)
        {
            return y is not null;
        }

        return !x.Equals(y);
    }
}
