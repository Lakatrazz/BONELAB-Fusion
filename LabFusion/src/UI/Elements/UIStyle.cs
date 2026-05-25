using UnityEngine;

using LabFusion.Extensions;

namespace LabFusion.UI.Elements;

public class UIStyle
{
    private Color _color = Color.white;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            NotifyStyleChanged();
        }
    }

    private Color _backgroundColor = Color.clear;
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            NotifyStyleChanged();
        }
    }

    private Texture _backgroundImage = null;
    public Texture BackgroundImage
    {
        get => _backgroundImage;
        set
        {
            _backgroundImage = value;
            NotifyStyleChanged();
        }
    }

    private float? _width = null;
    public float? Width
    {
        get => _width;
        set
        {
            _width = value;
            NotifyStyleChanged();
        }
    }

    private float? _height = null;
    public float? Height
    {
        get => _height;
        set
        {
            _height = value;
            NotifyStyleChanged();
        }
    }

    private UIDirection _direction = UIDirection.Column;
    public UIDirection Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            NotifyStyleChanged();
        }
    }

    private float _flexGrow = 0f;
    public float FlexGrow
    {
        get => _flexGrow;
        set
        {
            _flexGrow = value;
            NotifyStyleChanged();
        }
    }

    private UIAlign _alignItems = UIAlign.Stretch;
    public UIAlign AlignItems
    {
        get => _alignItems;
        set
        {
            _alignItems = value;
            NotifyStyleChanged();
        }
    }

    private UIRectOffset _margins = new();
    public UIRectOffset Margins
    {
        get => _margins;
        set
        {
            _margins = value;
            NotifyStyleChanged();
        }
    }

    private UIRectOffset _padding = new();
    public UIRectOffset Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            NotifyStyleChanged();
        }
    }

    public event Action StyleChanged;

    public UIStyle() { }

    public UIStyle(Action changeCallback)
    {
        StyleChanged += changeCallback;
    }

    private void NotifyStyleChanged() => StyleChanged?.InvokeSafe("executing StyleChanged event");
}
