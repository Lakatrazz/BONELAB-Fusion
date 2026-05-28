using LabFusion.UI.Styles;

using UnityEngine;

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

        styleSheet.Add(new StyleRule(new RootSelector(), fontStyle));

        var horizontalLineStyle = new Style
        {
            BackgroundColor = Color.white,
            Height = 2f,
            AlignSelfStretch = true,
        };

        styleSheet.Add(new StyleRule(new ClassSelector(CommonStyleClasses.HorizontalLine), horizontalLineStyle));

        var verticalLineStyle = new Style
        {
            BackgroundColor = Color.white,
            Width = 2f,
            AlignSelfStretch = true,
        };

        styleSheet.Add(new StyleRule(new ClassSelector(CommonStyleClasses.VerticalLine), verticalLineStyle));

        return styleSheet;
    }
}
