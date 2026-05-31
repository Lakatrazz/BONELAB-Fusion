namespace LabFusion.UI.Styles;

public enum TextAutoSizeMode
{
    None,

    BestFit,
}

public struct TextAutoSize
{
    public TextAutoSizeMode Mode { get; set; }

    public float MinSize { get; set; }

    public float MaxSize { get; set; }

    public TextAutoSize(float minSize, float maxSize) : this(TextAutoSizeMode.BestFit, minSize, maxSize) { }

    public TextAutoSize(TextAutoSizeMode mode, float minSize, float maxSize)
    {
        Mode = mode; 
        MinSize = minSize; 
        MaxSize = maxSize;
    }
}
