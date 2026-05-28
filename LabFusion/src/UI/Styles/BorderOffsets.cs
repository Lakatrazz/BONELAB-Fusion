namespace LabFusion.UI.Styles;

public struct BorderOffsets
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    public BorderOffsets(int left, int right, int top, int bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }
}
