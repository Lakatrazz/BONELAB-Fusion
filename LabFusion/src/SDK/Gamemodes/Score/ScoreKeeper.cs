using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public abstract class ScoreKeeper<TProperty>
{
    private readonly Dictionary<TProperty, MetadataInt> _propertyToScore = new();

    public event Action<TProperty, int> OnScoreChanged;

    private NetworkMetadata _metadata = null;

    public string Key { get; set; } = string.Empty;

    public abstract string GetKeyWithProperty(TProperty property);

    public abstract TProperty GetPropertyWithKey(string key);

    /// <summary>
    /// Registers the ScoreKeeper to a set NetworkMetadata with the default key of <see cref="CommonKeys.ScoreKey"/>.
    /// If you are using multiple ScoreKeepers, it is recommended to provide your own key so that they do not conflict.
    /// </summary>
    /// <param name="metadata">The metadata to register to.</param>
    public void Register(NetworkMetadata metadata) => Register(metadata, CommonKeys.ScoreKey);

    /// <summary>
    /// Registers the ScoreKeeper to a set NetworkMetadata with a specified key.
    /// </summary>
    /// <param name="metadata">The metadata to register to.</param>
    /// <param name="key">The key that will be used to look up scores.</param>
    public void Register(NetworkMetadata metadata, string key)
    {
        _metadata = metadata;
        _metadata.OnMetadataChanged += OnMetadataChanged;
        _metadata.OnMetadataRemoved += OnMetadataRemoved;

        Key = key;

        OnRegistered();
    }

    /// <summary>
    /// Unregisters the ScoreKeeper from its registered metadata.
    /// </summary>
    public void Unregister()
    {
        _metadata.OnMetadataChanged -= OnMetadataChanged;
        _metadata.OnMetadataRemoved -= OnMetadataRemoved;
        _metadata = null;

        OnUnregistered();
    }

    protected virtual void OnRegistered() { }

    protected virtual void OnUnregistered() { }

    private void OnMetadataChanged(string key, string value)
    {
        // Check if this is a score key
        if (!KeyHelper.KeyMatchesVariable(key, Key))
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

    private void OnMetadataRemoved(string key, string value)
    {
        // Check if this is a score key
        if (!KeyHelper.KeyMatchesVariable(key, Key))
        {
            return;
        }

        var removedMetadata = _propertyToScore.Where((pair) => pair.Value.Key == key);

        foreach (var pair in removedMetadata)
        {
            if (pair.Key == null)
            {
                continue;
            }

            _propertyToScore.Remove(pair.Key);
        }
    }

    public MetadataInt GetScoreMetadata(TProperty property)
    {
        if (property == null)
        {
            return null;
        }

        if (!_propertyToScore.TryGetValue(property, out var variable))
        {
            variable = new MetadataInt(GetKeyWithProperty(property), _metadata);
            _propertyToScore[property] = variable;
        }

        return variable;
    }

    public void RemoveScoreMetadata(TProperty property)
    {
        if (property == null)
        {
            return;
        }

        _propertyToScore.Remove(property);
    }

    public void SetScore(TProperty property, int score)
    {
        if (property == null)
        {
            return;
        }

        var variable = GetScoreMetadata(property);

        variable.SetValue(score);
    }

    public int GetScore(TProperty property)
    {
        if (property == null)
        {
            return 0;
        }

        var variable = GetScoreMetadata(property);

        return variable.GetValue();
    }

    public void AddScore(TProperty property, int amount = 1)
    {
        if (property == null)
        {
            return;
        }

        var score = GetScore(property) + amount;
        SetScore(property, score);
    }

    public void SubtractScore(TProperty property, int amount = 1, bool allowNegatives = false)
    {
        if (property == null)
        {
            return;
        }

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