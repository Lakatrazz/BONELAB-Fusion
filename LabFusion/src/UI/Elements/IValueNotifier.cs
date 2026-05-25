namespace LabFusion.UI.Elements;

public interface IValueNotifier<T>
{
    T Value { get; set; }

    event ValueChangedHandler<T> ValueChanged;

    void SetValueWithoutNotify(T value);
}
