using LabFusion.Network.Serialization;

namespace LabFusion.Player;

/// <summary>
/// A serializable reference to a PlayerId.
/// </summary>
public struct PlayerReference : INetSerializable
{
    public const int Size = sizeof(byte);

    public byte Id;

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Id);
    }

    public readonly bool TryGetPlayer(out PlayerId player)
    {
        player = GetPlayer();

        return player != null;
    }

    public readonly PlayerId GetPlayer()
    {
        return PlayerIdManager.GetPlayerId(Id);
    }

    public PlayerReference() : this(0) { }

    public PlayerReference(PlayerId player) : this(player.SmallId) { }

    public PlayerReference(byte id)
    {
        Id = id;
    }
}
