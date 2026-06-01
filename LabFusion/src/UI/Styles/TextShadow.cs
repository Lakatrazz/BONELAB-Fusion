using UnityEngine;

namespace LabFusion.UI.Styles;

public struct TextShadow
{
    public static readonly TextShadow None = new(Color.clear, 0f, 0f, 0f, 0f);

    public Color Color { get; set; }

    public float OffsetX { get; set; }

    public float OffsetY { get; set; }

    public float Dilate { get; set; }

    public float Softness { get; set; }

    public TextShadow(Color color, float offsetX, float offsetY, float dilate, float softness)
    {
        Color = color;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Dilate = dilate;
        Softness = softness;
    }
}
