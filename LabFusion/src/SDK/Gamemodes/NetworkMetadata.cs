using LabFusion.Data;

namespace LabFusion.SDK.Gamemodes
{
    public delegate bool MetadataSetDelegate(string key, string value);

    public delegate bool MetadataRemoveDelegate(string key);

    public class NetworkMetadata
    {
        private readonly FusionDictionary<string, string> _localDictionary = new();
        public FusionDictionary<string, string> LocalDictionary => _localDictionary;

        // Change callbacks
        public event Action<string, string> OnMetadataChanged;
        public event Action<string> OnMetadataRemoved;

        // Network request callbacks
        public MetadataSetDelegate OnTrySetMetadata;
        public MetadataRemoveDelegate OnTryRemoveMetadata;

        public bool TrySetMetadata(string key, string value)
        {
            if (OnTrySetMetadata == null)
            {
                return false;
            }

            return OnTrySetMetadata(key, value);
        }

        public bool TryRemoveMetadata(string key)
        {
            if (OnTryRemoveMetadata == null)
            {
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
                return value;

            return null;
        }

        public void ForceSetLocalMetadata(string key, string value)
        {
            _localDictionary[key] = value;

            OnMetadataChanged?.Invoke(key, value);
        }

        public void ForceRemoveLocalMetadata(string key)
        {
            if (_localDictionary.ContainsKey(key))
            {
                OnMetadataRemoved?.Invoke(key);

                _localDictionary.Remove(key);
            }
        }
    }
}
