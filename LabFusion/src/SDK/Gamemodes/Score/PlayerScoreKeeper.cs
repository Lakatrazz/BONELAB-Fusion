using LabFusion.Player;
using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public sealed class PlayerScoreKeeper : ScoreKeeper<PlayerId>
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