using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.SDK.Metadata;

public class MetadataProcessor
{
    public readonly List<string> HostAuthorityKeys = new();

    /// <summary>
    /// Checks if a player has authority to change a certain metadata key.
    /// Authority can be denied if a key is host-only and not able to be changed by players during gameplay.
    /// If the player ID is null, authority is also not given.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public bool HasAuthorityOverKey(string key, ClientSmallID? playerID)
    {
        if (!playerID.HasValue)
        {
            return false;
        }

        bool playerIsHost = playerID == PlayerIDManager.HostSmallID;

        if (HostAuthorityKeys.Contains(key) && !playerIsHost)
        {
            return false;
        }

        return true;
    }

    public MetadataProcessor WithHostAuthorityKey(string key)
    {
        HostAuthorityKeys.Add(key);
        return this;
    }
}
