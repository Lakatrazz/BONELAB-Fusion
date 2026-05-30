using LabFusion.Extensions;

namespace LabFusion.UI.Elements;

public abstract class FieldElement<T> : UIElement, IValueNotifier<T>
{
    private string _label = null;
    public string Label
    {
        get => _label;
        set
        {
            var previousLabel = _label;
            var newLabel = value;

            _label = newLabel;

            MarkContentDirty();

            LabelChanged?.InvokeSafe(previousLabel, newLabel, "executing LabelChanged event");
        }
    }

    private T _value = default;
    public T Value
    {
        get => _value;
        set
        {
            var previousValue = _value;
            var newValue = value;

            SetValueWithoutNotify(newValue);

            ValueChanged?.InvokeSafe(previousValue, newValue, "executing ValueChanged event");
        }
    }

    public event ValueChangedHandler<string> LabelChanged;

    public event ValueChangedHandler<T> ValueChanged;

    public void SetValueWithoutNotify(T value)
    {
        _value = value;
        MarkContentDirty();
    }
}
