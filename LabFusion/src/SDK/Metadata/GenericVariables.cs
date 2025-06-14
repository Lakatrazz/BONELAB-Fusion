using UnityEngine;

namespace LabFusion.SDK.Metadata;

public class MetadataInt : MetadataVariableT<int>
{
    public MetadataInt(string key, NetworkMetadata metadata) : base(key, metadata) { }

    public void Add(int value)
    {
        SetValue(GetValue() + value);
    }

    public void Subtract(int value)
    {
        SetValue(GetValue() - value);
    }
}

public class MetadataFloat : MetadataVariableT<float>
{
    public MetadataFloat(string key, NetworkMetadata metadata) : base(key, metadata) { }

    public void Add(float value)
    {
        SetValue(GetValue() + value);
    }

    public void Subtract(float value)
    {
        SetValue(GetValue() - value);
    }

    public void Multiply(float value)
    {
        SetValue(GetValue() * value);
    }

    public void Divide(float value)
    {
        SetValue(GetValue() / value);
    }
}

public class MetadataBool : MetadataVariableT<bool>
{
    public MetadataBool(string key, NetworkMetadata metadata) : base(key, metadata) { }

    public void Toggle()
    {
        SetValue(!GetValue());
    }
}

public class MetadataVector3 : MetadataVariable
{
    public MetadataVector3(string key, NetworkMetadata metadata) : base(key, metadata) { }

    public void SetValue(Vector3 value)
    {
        base.SetValue(JsonUtility.ToJson(value.BoxIl2CppObject()));
    }

    public new Vector3 GetValue()
    {
        return JsonUtility.FromJson<Vector3>(base.GetValue());
    }

    public void Add(Vector3 value)
    {
        SetValue(GetValue() + value);
    }

    public void Subtract(Vector3 value)
    {
        SetValue(GetValue() - value);
    }

    public void Multiply(float value)
    {
        SetValue(GetValue() * value);
    }

    public void Divide(float value)
    {
        SetValue(GetValue() / value);
    }
}