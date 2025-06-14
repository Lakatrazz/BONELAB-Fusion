namespace LabFusion.Network;

public enum ExpectedReceiverType
{
    /// <summary>
    /// This message is expected to be received on both clients and the server.
    /// </summary>
    Both,

    /// <summary>
    /// This message should only ever be received on the server.
    /// </summary>
    ServerOnly,

    /// <summary>
    /// This message should only ever be received on the clients.
    /// </summary>
    ClientsOnly,
}