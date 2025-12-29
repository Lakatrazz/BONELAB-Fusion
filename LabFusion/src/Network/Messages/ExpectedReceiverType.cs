namespace LabFusion.Network;

/// <summary>
/// The receiver that a message is expected to be handled on. 
/// This is checked before the message is relayed and can be used for validating sender authority.
/// </summary>
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