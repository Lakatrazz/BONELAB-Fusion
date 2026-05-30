using LabFusion.Extensions;

namespace LabFusion.UI.Elements;

public abstract class TextElement : UIElement, IValueNotifier<string>
{
    private string _text = null;
    public string Text { get => ((IValueNotifier<string>)this).Value; set => ((IValueNotifier<string>)this).Value = value; }

    string IValueNotifier<string>.Value
    {
        get => _text;
        set
        {
            var previousValue = _text;
            var newValue = value;

            ((IValueNotifier<string>)this).SetValueWithoutNotify(newValue);

            ValueChanged?.InvokeSafe(previousValue, newValue, "executing ValueChanged event");
        }
    }

    public event ValueChangedHandler<string> ValueChanged;

    void IValueNotifier<string>.SetValueWithoutNotify(string value)
    {
        _text = value;
        MarkContentDirty();
    }
}
