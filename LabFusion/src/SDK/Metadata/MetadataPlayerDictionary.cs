using LabFusion.Player;

namespace LabFusion.SDK.Metadata;

public class MetadataPlayerDictionary<TVariable> : MetadataDictionary<PlayerId, TVariable> where TVariable : MetadataVariable
{
    public override string GetKeyWithProperty(PlayerId property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, property);
    }

    public override PlayerId GetPropertyWithKey(string key)
    {
        return KeyHelper.GetPlayerFromKey(key);
    }
}
