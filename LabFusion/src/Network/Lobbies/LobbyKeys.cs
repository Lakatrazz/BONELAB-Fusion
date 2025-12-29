namespace LabFusion.Network;

/// <summary>
/// Common keys used for lobby metadata.
/// </summary>
public static class LobbyKeys
{
    private const string _internalPrefix = "BONELAB_FUSION_";
    public const string HasServerOpenKey = _internalPrefix + "HasServerOpen";

    public const string KeyCollectionKey = _internalPrefix + "KeyCollection";

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
