using UnityEngine;

namespace LabFusion.UI.Styles;

public struct TextOutline
{
    public static readonly TextOutline None = new(Color.clear, 0f, 0f);

    public Color Color { get; set; }

    public float Width { get; set; }

    public float Softness { get; set; }

    public TextOutline(float width) : this(Color.black, width, 0f) { }

    public TextOutline(Color color, float width, float softness)
    {
        Color = color;
        Width = width;
        Softness = softness;
    }
}
