using LabFusion.Marrow.Proxies;
using LabFusion.Preferences;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuChaining
{
    public static EnumElement AsPref<TEnum>(this EnumElement element, FusionPref<TEnum> pref, Action<TEnum> onValueChanged = null) where TEnum : Enum
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
            if (element.Value.ToString() != value.ToString())
            {
                element.Value = value;
            }

            onValueChanged?.Invoke(value);
        };

        element.OnCleared += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static StringElement AsPref(this StringElement element, FusionPref<string> pref, Action<string> onValueChanged = null)
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

        element.OnCleared += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static FloatElement AsPref(this FloatElement element, FusionPref<float> pref, Action<float> onValueChanged = null)
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

        element.OnCleared += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static IntElement AsPref(this IntElement element, FusionPref<int> pref, Action<int> onValueChanged = null)
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

        element.OnCleared += () =>
        {
            element.OnValueChanged -= OnButtonChanged;
        };

        return element;
    }

    public static BoolElement AsPref(this BoolElement element, FusionPref<bool> pref, Action<bool> onValueChanged = null)
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

        element.OnCleared += () =>
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

    public static FunctionElement Do(this FunctionElement element, Action onPressed)
    {
        element.OnPressed += onPressed;

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

    public static TElement WithTitle<TElement>(this TElement element, string title) where TElement : MenuElement
    {
        element.Title = title;

        return element;
    }

    public static TLabel WithColor<TLabel>(this TLabel element, Color color) where TLabel : LabelElement
    {
        element.Color = color;

        return element;
    }

    public static TButton WithInteractability<TButton>(this TButton button, bool interactable) where TButton : ButtonElement
    {
        button.Interactable = interactable;

        return button;
    }

    public static TElement Cleared<TElement>(this TElement element) where TElement : MenuElement
    {
        element.Clear();

        return element;
    }
}