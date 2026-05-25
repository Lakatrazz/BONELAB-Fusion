using UnityEngine;

using LabFusion.Extensions;

using Il2CppTMPro;

namespace LabFusion.UI.Elements;

public class UIStyle
{
    private Color _textColor = Color.white;
    public Color TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            NotifyStyleChanged();
        }
    }

    private UIVertexColor? _textGradient = null;
    public UIVertexColor? TextGradient
    {
        get => _textGradient;
        set
        {
            _textGradient = value;
            NotifyStyleChanged();
        }
    }

    private TMP_FontAsset _font = null;
    public TMP_FontAsset Font
    {
        get => _font;
        set
        {
            _font = value;
            NotifyStyleChanged();
        }
    }

    private float? _fontSize = null;
    public float? FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
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

    private UIJustify _justifyContent = UIJustify.Start;
    public UIJustify JustifyContent
    {
        get => _justifyContent;
        set
        {
            _justifyContent = value;
            NotifyStyleChanged();
        }
    }

    private UIAlign _alignContent = UIAlign.Stretch;
    public UIAlign AlignContent
    {
        get => _alignContent;
        set
        {
            _alignContent = value;
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
