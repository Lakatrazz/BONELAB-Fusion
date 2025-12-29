namespace LabFusion.Network;

/// <summary>
/// Common keys used for lobby metadata.
/// </summary>
public static class LobbyKeys
{
    /// <summary>
    /// The key to identify that this is a Fusion lobby.
    /// </summary>
    public const string IdentifierKey = "MarrowFusion";

    /// <summary>
    /// The key to identify that the lobby is open and joinable.
    /// </summary>
    public const string HasLobbyOpenKey = "HasLobbyOpen";

    /// <summary>
    /// The key to identify the array containing all keys for the lobby.
    /// </summary>
    public const string KeyCollectionKey = "KeyCollection";

    /// <summary>
    /// The key for a lobby's code. The value should always be uppercase to allow for case insensitivity.
    /// </summary>
    public const string LobbyCodeKey = "LobbyCode";

    /// <summary>
    /// The key for a lobby's privacy.
    /// </summary>
    public const string PrivacyKey = "Privacy";

    /// <summary>
    /// The key to get if a lobby is full.
    /// </summary>
    public const string FullKey = "Full";

    /// <summary>
    /// The key for a lobby's major version.
    /// </summary>
    public const string VersionMajorKey = "VersionMajor";

    /// <summary>
    /// The key for a lobby's minor version.
    /// </summary>
    public const string VersionMinorKey = "VersionMinor";

    /// <summary>
    /// The key for a lobby's game.
    /// </summary>
    public const string GameKey = "Game";
}
