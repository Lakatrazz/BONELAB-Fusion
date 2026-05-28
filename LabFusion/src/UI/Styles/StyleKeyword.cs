namespace LabFusion.UI.Styles;

public enum StyleKeyword
{
    /// <summary>
    /// Indicates that a specific value has been set for a style to use.
    /// </summary>
    Undefined,

    /// <summary>
    /// Indicates that no style override has been set, so the value determined by the style sheet will be used.
    /// </summary>
    Null,

    /// <summary>
    /// Indicates that the style has been set to use its default value.
    /// </summary>
    Initial,
}
