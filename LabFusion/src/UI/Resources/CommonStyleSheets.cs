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

    private static StyleSheet _watchStyleSheet = null;
    public static StyleSheet WatchStyleSheet
    {
        get
        {
            _watchStyleSheet ??= CreateWatchStyleSheet();
            return _watchStyleSheet;
        }
    }

    public static void MarkDirty()
    {
        _defaultStyleSheet = null;
        _watchStyleSheet = null;
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

    private static StyleSheet CreateWatchStyleSheet()
    {
        var styleSheet = new StyleSheet();

        var glowStyle = new Style
        {
            TextGlow = new TextGlow(new Color(0f, 0.4f, 2f, 1f), 0f, 0f, 1f, 0.1f),
        };
        styleSheet.Add(new StyleRule(new RootSelector(), glowStyle));

        return styleSheet;
    }
}
