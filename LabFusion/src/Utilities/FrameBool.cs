namespace LabFusion.Utilities;

/// <summary>
/// A boolean that can only be true for a single frame.
/// </summary>
public readonly struct FrameBool
{
    private readonly bool _value;
    public bool Value => _value && Valid;

    private readonly int _frame;
    public int Frame => _frame;

    public bool Valid => TimeUtilities.FrameCount == Frame;

    public FrameBool(bool value)
    {
        _value = value;
        _frame = TimeUtilities.FrameCount;
    }

    public static implicit operator bool(FrameBool value) => value.Value;

    public static implicit operator FrameBool(bool value) => new(value);
}
