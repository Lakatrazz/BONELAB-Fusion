namespace LabFusion.Menu.Data;

public class EnumElementData : ElementData
{
    private Enum _value = null;

    public Enum Value
    {
        get { return _value; }
        set
        {
            _value = value;
            OnValueChanged?.Invoke(value);
        }
    }

    public Type EnumType { get; set; } = null;

    public Action<Enum> OnValueChanged;
}