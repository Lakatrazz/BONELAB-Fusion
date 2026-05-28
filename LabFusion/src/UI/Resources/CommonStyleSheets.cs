using LabFusion.UI.Styles;

namespace LabFusion.UI.Resources;

public static class CommonStyleSheets
{
    private static StyleSheet _defaultStyleSheet = null;
    public static StyleSheet DefaultStyleSheet
    {
        get
        {
            _defaultStyleSheet ??= CreateDefaultStyleSheet();
            return _defaultStyleSheet;
        }
    }

    public static void MarkDirty()
    {
        _defaultStyleSheet = null;
    }

    private static StyleSheet CreateDefaultStyleSheet()
    {
        var styleSheet = new StyleSheet();

        var fontStyle = new Style
        {
            Font = UIResources.GetCommonFont(CommonFonts.BalooBhai2_SemiBold)
        };

        var fontRule = new StyleRule(new UniversalSelector(), fontStyle);
        styleSheet.Add(fontRule);

        return styleSheet;
    }
}
