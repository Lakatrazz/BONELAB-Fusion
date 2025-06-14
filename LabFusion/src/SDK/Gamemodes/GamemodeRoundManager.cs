using Il2CppSLZ.Marrow.SceneStreaming;

using LabFusion.Data;
using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.SDK.Gamemodes;

[Serializable]
public class GamemodeLevelRotations
{
    [JsonPropertyName("gamemodeBarcode")]
    public string GamemodeBarcode { get; set; } = null;

    [JsonPropertyName("levelRotations")]
    public List<LevelRotation> LevelRotations { get; set; } = new();
}

[Serializable]
public class GamemodeRoundSettings
{
    [JsonPropertyName("levelRotations")]
    public List<GamemodeLevelRotations> LevelRotations { get; set; } = new();

    [JsonPropertyName("roundsBeforeNextLevel")]
    public int RoundsBeforeNextLevel { get; set; } = 3;

    [JsonPropertyName("shuffleLevels")]
    public bool ShuffleLevels { get; set; } = true;

    [JsonPropertyName("timeBetweenRounds")]
    public int TimeBetweenRounds { get; set; } = 30;

    public GamemodeLevelRotations GetRotationsByGamemode(string gamemodeBarcode)
    {
        foreach (var rotation in LevelRotations)
        {
            if (rotation.GamemodeBarcode == gamemodeBarcode)
            {
                return rotation;
            }
        }

        var rotations = new GamemodeLevelRotations()
        {
            GamemodeBarcode = gamemodeBarcode,
        };

        LevelRotations.Add(rotations);

        return rotations;
    }
}

public static class GamemodeRoundManager
{
    private static GamemodeRoundSettings _settings = new();
    public static GamemodeRoundSettings Settings => _settings;

    public static LevelRotation ActiveRotation { get; set; } = null;

    public const string FileName = "gamemodeRoundSettings.json";

    private static int _rotationRounds = 0;

    private static int _levelIndex = 0;

    internal static void OnInitializeMelon()
    {
        LoadSettings();

        GamemodeManager.OnGamemodeStopped += OnGamemodeStopped;
        GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
        GamemodeManager.OnGamemodeUnready += OnGamemodeUnready;
    }

    private static void OnGamemodeUnready()
    {
        _rotationRounds = 0;
    }

    private static void OnGamemodeChanged(Gamemode gamemode)
    {
        _rotationRounds = 0;

        if (gamemode != null && ActiveRotation != null)
        {
            var rotations = Settings.GetRotationsByGamemode(gamemode.Barcode);

            if (!rotations.LevelRotations.Contains(ActiveRotation))
            {
                ActiveRotation = null;
            }
        }
    }

    private static void OnGamemodeStopped()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveRotation != null)
        {
            _rotationRounds++;

            if (_rotationRounds >= Settings.RoundsBeforeNextLevel)
            {
                _rotationRounds = 0;
                NextLevel();
            }
        }
    }

    private static void NextLevel()
    {
        if (ActiveRotation == null || ActiveRotation.LevelBarcodes.Count <= 0)
        {
            return;
        }

        if (Settings.ShuffleLevels)
        {
            _levelIndex = PickRandomIndex(ActiveRotation.LevelBarcodes.Count, _levelIndex);
        }
        else
        {
            _levelIndex = (_levelIndex + 1) % ActiveRotation.LevelBarcodes.Count;
        }

        var levelBarcode = ActiveRotation.LevelBarcodes[_levelIndex];

        SceneStreamer.Load(new(levelBarcode));
    }

    private static int PickRandomIndex(int count, int previousIndex)
    {
        for (var i = 0; i < 3; i++)
        {
            var newIndex = UnityEngine.Random.Range(0, count);

            if (newIndex != previousIndex)
            {
                return newIndex;
            }
        }

        return previousIndex;
    }

    public static void SaveSettings()
    {
        DataSaver.WriteJsonToFile(FileName, Settings);
    }

    public static void LoadSettings()
    {
        var loadedSettings = DataSaver.ReadJsonFromFile<GamemodeRoundSettings>(FileName);

        if (loadedSettings != null)
        {
            _settings = loadedSettings;
        }
    }
}
