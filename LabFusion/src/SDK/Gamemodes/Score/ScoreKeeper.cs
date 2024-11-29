using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public abstract class ScoreKeeper<TProperty>
{
    private readonly Dictionary<TProperty, MetadataInt> _propertyToScore = new();

    public event Action<TProperty, int> OnScoreChanged;

    private NetworkMetadata _metadata = null;

    public abstract string GetKey();

    public abstract string GetKeyWithProperty(TProperty property);

    public abstract TProperty GetPropertyWithKey(string key);

    public void Register(NetworkMetadata metadata)
    {
        _metadata = metadata;
        _metadata.OnMetadataChanged += OnMetadataChanged;
    }

    public void Unregister()
    {
        _metadata.OnMetadataChanged -= OnMetadataChanged;
        _metadata = null;
    }

    private void OnMetadataChanged(string key, string value)
    {
        // Check if this is a score key
        if (!KeyHelper.KeyMatchesVariable(key, GetKey()))
        {
            return;
        }

        var property = GetPropertyWithKey(key);

        // If the property doesn't exist, don't invoke a score change
        if (property == null)
        {
            return;
        }

        int score = 0;

        if (int.TryParse(value, out var parsedScore))
        {
            score = parsedScore;
        }

        OnScoreChanged?.Invoke(property, score);
    }

    public MetadataInt GetScoreMetadata(TProperty property)
    {
        if (!_propertyToScore.TryGetValue(property, out var variable))
        {
            variable = new MetadataInt(GetKeyWithProperty(property), _metadata);
            _propertyToScore[property] = variable;
        }

        return variable;
    }

    public void SetScore(TProperty property, int score)
    {
        var variable = GetScoreMetadata(property);

        variable.SetValue(score);
    }

    public int GetScore(TProperty property)
    {
        var variable = GetScoreMetadata(property);

        return variable.GetValue();
    }

    public void AddScore(TProperty property, int amount = 1)
    {
        var score = GetScore(property) + amount;
        SetScore(property, score);
    }

    public void SubtractScore(TProperty property, int amount = 1, bool allowNegatives = false)
    {
        var score = GetScore(property) - amount;

        if (!allowNegatives && score < 0)
        {
            score = 0;
        }

        SetScore(property, score);
    }

    public int GetTotalScore()
    {
        int totalScore = 0;

        foreach (var variable in _propertyToScore.Values) 
        { 
            totalScore += variable.GetValue();
        }

        return totalScore;
    }

    public void ResetScores()
    {
        foreach (var value in _propertyToScore.Values)
        {
            value.SetValue(0);

            value.Remove();
        }
    }
}