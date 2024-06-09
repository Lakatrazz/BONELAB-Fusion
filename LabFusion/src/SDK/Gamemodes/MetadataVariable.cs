namespace LabFusion.SDK.Gamemodes
{
    public class MetadataVariable
    {
        public NetworkMetadata Metadata { get; }
        public string Key { get; }

        public MetadataVariable(string key, NetworkMetadata metadata)
        {
            Key = key;
            Metadata = metadata;
        }

        public void SetValue(string value)
        {
            Metadata.TrySetMetadata(Key, value);
        }

        public void SetValue<TValue>(TValue value)
        {

        }

        public string GetValue()
        {
            return Metadata.GetMetadata(Key);
        }

        public TValue GetValue<TValue>()
        {
            return default;
        }
    }
}
