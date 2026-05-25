using UnityEngine;

namespace LabFusion.UI.Elements;

public struct UIVertexColor
{
    public Color TopLeft { get; set; }
    public Color TopRight { get; set; }
    public Color BottomLeft { get; set; }
    public Color BottomRight { get; set; }

    public UIVertexColor(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    public static UIVertexColor CreateHorizontalGradient(Color left, Color right) => new(left, right, left, right);

    public static UIVertexColor CreateVerticalGradient(Color top, Color bottom) => new(top, top, bottom, bottom);

    public static UIVertexColor CreateCrossGradient(Color up, Color down) => new(down, up, up, down);
}
