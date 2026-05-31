namespace LabFusion.UI.Styles;

public enum LengthUnit
{
    Pixel,

    Ratio,
}

public struct Length
{
    public float Value { get; set; }

    public LengthUnit Unit { get; set; }

    public Length(float value) : this(value, LengthUnit.Pixel) { }

    public Length(float value, LengthUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public readonly Length ToPixels(float inheritedPixels)
    {
        float pixels = Unit switch
        {
            LengthUnit.Ratio => Value * inheritedPixels,
            _ => Value,
        };

        return FromPixels(pixels);
    } 

    public static Length FromPixels(float pixels) => new(pixels, LengthUnit.Pixel);

    public static Length FromRatio(float ratio) => new(ratio, LengthUnit.Ratio);

    public static implicit operator Length(float value) => FromPixels(value);

    public static implicit operator float(Length value) => value.Value;
}
