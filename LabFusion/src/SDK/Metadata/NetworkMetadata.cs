using LabFusion.Utilities;

namespace LabFusion.SDK.Metadata;

public delegate bool MetadataSetDelegate(string key, string value);

public delegate bool MetadataRemoveDelegate(string key);

public class NetworkMetadata
{
    private readonly Dictionary<string, string> _localDictionary = new();
    public Dictionary<string, string> LocalDictionary => _localDictionary;

    // Change callbacks
    public event Action<string, string> OnMetadataChanged, OnMetadataRemoved;

    // Network request callbacks
    public MetadataSetDelegate OnTrySetMetadata;
    public MetadataRemoveDelegate OnTryRemoveMetadata;

    public bool TrySetMetadata(string key, string value)
    {
        if (OnTrySetMetadata == null)
        {
            FusionLogger.Warn($"Tried setting metadata with a key of {key} and value of {value}, but the request callbacks were not set!");
            return false;
        }

        return OnTrySetMetadata(key, value);
    }

    public bool TryRemoveMetadata(string key)
    {
        if (OnTryRemoveMetadata == null)
        {
            FusionLogger.Warn($"Tried removing metadata with a key of {key}, but the request callbacks were not set!");
            return false;
        }

        return OnTryRemoveMetadata(key);
    }

    public bool TryGetMetadata(string key, out string value)
    {
        return _localDictionary.TryGetValue(key, out value);
    }

    public string GetMetadata(string key)
    {
        if (_localDictionary.TryGetValue(key, out string value))
        {
            return value;
        }

        return null;
    }

    public void ForceSetLocalMetadata(string key, string value)
    {
        _localDictionary[key] = value;

        OnMetadataChanged?.InvokeSafe(key, value, "executing OnMetadataChanged");
    }

    public void ForceRemoveLocalMetadata(string key)
    {
        if (_localDictionary.TryGetValue(key, out var value))
        {
            OnMetadataRemoved?.InvokeSafe(key, value, "executing OnMetadataRemoved");

            _localDictionary.Remove(key);
        }
    }

    public void ClearLocalMetadata()
    {
        var keys = _localDictionary.Keys.ToArray();

        foreach (var key in keys)
        {
            OnMetadataRemoved?.InvokeSafe(key, _localDictionary[key], "executing OnMetadataRemoved");

            _localDictionary.Remove(key);
        }
    }

    public void ClearLocalMetadataExcept(Predicate<string> predicate)
    {
        var keys = _localDictionary.Keys.ToArray();

        foreach (var key in keys)
        {
            if (predicate(key))
            {
                continue;
            }

            OnMetadataRemoved?.InvokeSafe(key, _localDictionary[key], "executing OnMetadataRemoved");

            _localDictionary.Remove(key);
        }
    }
}