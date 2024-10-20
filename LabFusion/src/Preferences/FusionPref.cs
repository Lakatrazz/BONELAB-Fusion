using MelonLoader;

using LabFusion.Preferences.Server;

namespace LabFusion.Preferences;

public enum PrefUpdateMode
{
    IGNORE = 0,
    SERVER_UPDATE = 1,
    CLIENT_UPDATE = 2,
    LOCAL_UPDATE = 3,
}

public interface IFusionPref<T>
{
    Action<T> OnValueChanged { get; set; }

    T Value { get; set; }
}

public class ReadOnlyPref<T> : IFusionPref<T>
{
    private readonly T _value;

    public Action<T> OnValueChanged { get; set; }

    public ReadOnlyPref(T value)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
        }
    }
}

public class FusionPref<T> : IFusionPref<T>
{
    private readonly MelonPreferences_Category _category;
    private readonly MelonPreferences_Entry<T> _entry;
    private readonly PrefUpdateMode _mode;

    public Action<T> OnValueChanged { get; set; }

    public T Value
    {
        get
        {
            return _entry.Value;
        }
        set
        {
            _entry.Value = value;
            OnValueChanged?.Invoke(value);
            _category.SaveToFile(false);
            PushUpdate();
        }
    }

    public FusionPref(MelonPreferences_Category category, string name, T defaultValue, PrefUpdateMode mode = PrefUpdateMode.IGNORE)
    {
        _category = category;
        _entry = category.CreateEntry<T>(name, defaultValue);
        _mode = mode;

        FusionPreferences.OnPrefsLoaded += OnPrefsLoaded;
    }

    public void OnPrefsLoaded()
    {
        OnValueChanged?.Invoke(Value);
        PushUpdate();
    }

    private void PushUpdate()
    {
        switch (_mode)
        {
            default:
            case PrefUpdateMode.IGNORE:
                break;
            case PrefUpdateMode.SERVER_UPDATE:
                FusionPreferences.SendServerSettings();
                ServerSettingsManager.PushSettingsUpdate();
                break;
            case PrefUpdateMode.CLIENT_UPDATE:
                FusionPreferences.SendClientSettings();
                break;
            case PrefUpdateMode.LOCAL_UPDATE:
                ServerSettingsManager.PushSettingsUpdate();
                break;
        }
    }
}