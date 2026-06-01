using UnityEngine;

namespace LabFusion.UI.Styles;

public struct TextGlow
{
    public static readonly TextGlow None = new(Color.clear, 0f, 0f, 0f, 0f);

    public Color Color { get; set; }

    public float Offset { get; set; }

    public float Inner { get; set; }

    public float Outer { get; set; }

    public float Power { get; set; }

    public TextGlow(Color color, float offset, float inner, float outer, float power)
    {
        Color = color;
        Offset = offset;
        Inner = inner;
        Outer = outer;
        Power = power;
    }
}
