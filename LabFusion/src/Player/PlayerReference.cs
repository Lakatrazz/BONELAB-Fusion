using LabFusion.Network;
using LabFusion.Network.Serialization;

namespace LabFusion.Player;

/// <summary>
/// A serializable reference to a PlayerID.
/// </summary>
public struct PlayerReference : INetSerializable
{
    public ClientSmallID SmallID;

    public readonly int? GetSize() => SmallID.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SmallID);
    }

    public readonly bool TryGetPlayer(out PlayerID player)
    {
        player = GetPlayer();

        return player != null;
    }

    public readonly PlayerID GetPlayer()
    {
        return PlayerIDManager.GetPlayerID(SmallID);
    }

    public PlayerReference() : this(ClientSmallID.Empty) { }

    public PlayerReference(PlayerID player) : this(player.SmallID) { }

    public PlayerReference(ClientSmallID smallID)
    {
        SmallID = smallID;
    }
}
