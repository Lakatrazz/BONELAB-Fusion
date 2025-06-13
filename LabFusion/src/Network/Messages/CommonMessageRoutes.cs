namespace LabFusion.Network;

public static class CommonMessageRoutes
{
    /// <summary>
    /// Use this route when not using a relay, such as when a player hasn't properly joined the server yet.
    /// </summary>
    public static readonly MessageRoute None = new(RelayType.None, NetworkChannel.Reliable);

    public static readonly MessageRoute ReliableToServer = new(RelayType.ToServer, NetworkChannel.Reliable);

    public static readonly MessageRoute ReliableToClients = new(RelayType.ToClients, NetworkChannel.Reliable);

    public static readonly MessageRoute UnreliableToClients = new(RelayType.ToClients, NetworkChannel.Unreliable);

    public static readonly MessageRoute ReliableToOtherClients = new(RelayType.ToOtherClients, NetworkChannel.Reliable);

    public static readonly MessageRoute UnreliableToOtherClients = new(RelayType.ToOtherClients, NetworkChannel.Unreliable);
}
