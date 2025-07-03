using UnityEngine;

namespace LabFusion.Utilities;

public static class GameHelper
{
    private static readonly string _gameNameCached = Application.productName;
    public static string GameName => _gameNameCached;
}
