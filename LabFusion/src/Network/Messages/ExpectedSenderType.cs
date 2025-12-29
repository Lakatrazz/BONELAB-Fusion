namespace LabFusion.Network;

public enum ExpectedSenderType
{
    /// <summary>
    /// This message is expected to be sent by both clients and the server.
    /// </summary>
    Both,

    /// <summary>
    /// This message should only ever be sent by the server.
    /// </summary>
    ServerOnly,

    /// <summary>
    /// This message should only ever be sent by the clients.
    /// </summary>
    ClientsOnly,
}