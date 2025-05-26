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

    public readonly bool TryGetPlayer(out PlayerID player)
    {
        player = GetPlayer();

        return player != null;
    }

    public readonly PlayerID GetPlayer()
    {
        return PlayerIDManager.GetPlayerID(Id);
    }

    public PlayerReference() : this(0) { }

    public PlayerReference(PlayerID player) : this(player.SmallID) { }

    public PlayerReference(byte id)
    {
        Id = id;
    }
}
