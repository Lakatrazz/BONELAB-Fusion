namespace LabFusion.Network;

public static class LobbyKeys
{
    private const string _internalPrefix = "BONELAB_FUSION_";
    public const string HasServerOpenKey = _internalPrefix + "HasServerOpen";

    public const string KeyCollectionKey = _internalPrefix + "KeyCollection";

    public const string LobbyCodeKey = "LobbyCode";

    public const string PrivacyKey = "Privacy";

    public const string GameKey = "Game";
}
