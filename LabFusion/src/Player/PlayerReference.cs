using LabFusion.Network.Serialization;

namespace LabFusion.Player;

/// <summary>
/// A serializable reference to a PlayerID.
/// </summary>
public struct PlayerReference : INetSerializable
{
    public const int Size = sizeof(byte);

    public byte ID;

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ID);
    }

    public readonly bool TryGetPlayer(out PlayerID player)
    {
        player = GetPlayer();

        return player != null;
    }

    public readonly PlayerID GetPlayer()
    {
        return PlayerIDManager.GetPlayerID(ID);
    }

    public PlayerReference() : this(0) { }

    public PlayerReference(PlayerID player) : this(player.SmallID) { }

    public PlayerReference(byte id)
    {
        ID = id;
    }
}
