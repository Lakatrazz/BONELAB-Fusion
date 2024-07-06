namespace LabFusion.SDK.Metadata;

using System.Text.Json;

public class MetadataVariable
{
    public NetworkMetadata Metadata { get; }
    public string Key { get; }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true,
    };

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
        SetValue(JsonSerializer.Serialize(value, SerializerOptions));
    }

    public string GetValue()
    {
        return Metadata.GetMetadata(Key);
    }

    public TValue GetValue<TValue>()
    {
        string value = GetValue();

        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TValue>(value, SerializerOptions);
    }
}

public class MetadataVariableT<TValue> : MetadataVariable
{
    public MetadataVariableT(string key, NetworkMetadata metadata) : base(key, metadata) { }

    public void SetValue(TValue value)
    {
        base.SetValue<TValue>(value);
    }

    public new TValue GetValue()
    {
        return base.GetValue<TValue>();
    }
}