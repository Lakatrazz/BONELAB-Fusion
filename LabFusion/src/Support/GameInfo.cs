using UnityEngine;

namespace LabFusion.Support;

public static class GameInfo
{
    private static readonly string _gameNameCached = Application.productName;
    public static string GameName => _gameNameCached;
}
