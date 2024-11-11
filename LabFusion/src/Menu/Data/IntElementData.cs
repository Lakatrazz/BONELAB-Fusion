namespace LabFusion.Menu.Data;

public class IntElementData : ElementData
{
    private int _value = 0;
    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;

            OnValueChanged?.Invoke(value);
        }
    }

    public int MinValue { get; set; } = 0;

    public int MaxValue { get; set; } = 1;

    public int Increment { get; set; } = 1;

    public Action<int> OnValueChanged;
}
