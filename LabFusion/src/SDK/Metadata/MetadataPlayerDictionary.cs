using LabFusion.Player;

namespace LabFusion.SDK.Metadata;

public class MetadataPlayerDictionary<TVariable> : MetadataDictionary<PlayerID, TVariable> where TVariable : MetadataVariable
{
    public override string GetKeyWithProperty(PlayerID property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, property);
    }

    public override PlayerID GetPropertyWithKey(string key)
    {
        return KeyHelper.GetPlayerFromKey(key);
    }
}
