using LabFusion.Marrow.Proxies;
using LabFusion.Preferences;
using static Il2CppSystem.Linq.Expressions.Interpreter.NullableMethodCallInstruction;

namespace LabFusion.Menu;

public static class MenuChaining
{
    public static EnumElement AsPref<TEnum>(this EnumElement element, IFusionPref<TEnum> pref, Action<TEnum> onValueChanged = null) where TEnum : Enum
    {
        element.Value = pref.Value;
        element.EnumType = typeof(TEnum);

        void OnButtonChanged(Enum value)
        {
            pref.Value = (TEnum)value;
        };

        element.OnValueChanged += OnButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (element.Value != value as Enum)
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnDestroyed += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static StringElement AsPref(this StringElement element, IFusionPref<string> pref, Action<string> onValueChanged = null)
    {
        element.Value = pref.Value;

        void OnButtonChanged(string value)
        {
            pref.Value = value;
        };

        element.OnValueChanged += OnButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (element.Value != value)
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnDestroyed += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static FloatElement AsPref(this FloatElement element, IFusionPref<float> pref, Action<float> onValueChanged = null)
    {
        element.Value = pref.Value;

        void OnButtonChanged(float value)
        {
            pref.Value = value;
        };

        element.OnValueChanged += OnButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (element.Value != value)
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnDestroyed += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static IntElement AsPref(this IntElement element, IFusionPref<int> pref, Action<int> onValueChanged = null)
    {
        element.Value = pref.Value;

        void OnButtonChanged(int value)
        {
            pref.Value = value;
        };

        element.OnValueChanged += OnButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (element.Value != value)
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnDestroyed += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static BoolElement AsPref(this BoolElement element, IFusionPref<bool> pref, Action<bool> onValueChanged = null)
    {
        element.Value = pref.Value;

        void OnButtonChanged(bool value)
        {
            pref.Value = value;
        };

        element.OnValueChanged += OnButtonChanged;

        pref.OnValueChanged += (value) =>
        {
            // Update the value
            if (element.Value != value)
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnDestroyed += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static FunctionElement Link(this FunctionElement element, PageElement page)
    {
        element.OnPressed += page.Select;

        return element;
    }

    public static IntElement WithLimits(this IntElement element, int minValue, int maxValue) 
    {
        element.MinValue = minValue;
        element.MaxValue = maxValue;

        return element;
    }

    public static IntElement WithIncrement(this IntElement element, int increment)
    {
        element.Increment = increment;

        return element;
    }

    public static FloatElement WithLimits(this FloatElement element, float minValue, float maxValue)
    {
        element.MinValue = minValue;
        element.MaxValue = maxValue;

        return element;
    }

    public static FloatElement WithIncrement(this FloatElement element, float increment)
    {
        element.Increment = increment;

        return element;
    }
}