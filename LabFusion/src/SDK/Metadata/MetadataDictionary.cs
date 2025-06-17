using LabFusion.Utilities;

namespace LabFusion.SDK.Metadata;

public abstract class MetadataDictionary<TProperty, TVariable> where TVariable : MetadataVariable
{
    private readonly Dictionary<TProperty, TVariable> _propertyToVariable = new();

    public event Action<TProperty, TVariable> OnVariableChanged;

    private NetworkMetadata _metadata = null;

    public string Key { get; set; } = string.Empty;

    public abstract string GetKeyWithProperty(TProperty property);

    public abstract TProperty GetPropertyWithKey(string key);

    public void Register(NetworkMetadata metadata, string key)
    {
        _metadata = metadata;
        _metadata.OnMetadataChanged += OnMetadataChanged;
        _metadata.OnMetadataRemoved += OnMetadataRemoved;

        Key = key;

        OnRegistered();
    }

    public void Unregister()
    {
        _metadata.OnMetadataChanged -= OnMetadataChanged;
        _metadata = null;

        Key = string.Empty;

        OnUnregistered();
    }

    protected virtual void OnRegistered() { }

    protected virtual void OnUnregistered() { }

    private void OnMetadataChanged(string key, string value)
    {
        // Check if this is a variable key
        if (!KeyHelper.KeyMatchesVariable(key, Key))
        {
            return;
        }

        var property = GetPropertyWithKey(key);

        // If the property doesn't exist, don't invoke a variable change
        if (property == null)
        {
            return;
        }

        var variable = GetVariable(property);

        OnVariableChanged?.InvokeSafe(property, variable, "executing MetadataDictionary.OnVariableChanged");
    }

    private void OnMetadataRemoved(string key, string value)
    {
        // Check if this is a variable key
        if (!KeyHelper.KeyMatchesVariable(key, Key))
        {
            return;
        }

        var removedMetadata = _propertyToVariable.Where(pair => pair.Value.Key == key).ToList();

        foreach (var removed in removedMetadata)
        {
            if (removed.Key == null)
            {
                continue;
            }

            _propertyToVariable.Remove(removed.Key);
        }
    }

    public TVariable GetVariable(TProperty property)
    {
        if (property == null)
        {
            return null;
        }

        if (!_propertyToVariable.TryGetValue(property, out var variable))
        {
            variable = Activator.CreateInstance(typeof(TVariable), GetKeyWithProperty(property), _metadata) as TVariable;
            _propertyToVariable[property] = variable;
        }

        return variable;
    }

    public void RemoveVariable(TProperty property)
    {
        if (property == null)
        {
            return;
        }

        _propertyToVariable.Remove(property);
    }

    public void Clear()
    {
        foreach (var value in _propertyToVariable.Values)
        {
            value.Remove();
        }
    }
}