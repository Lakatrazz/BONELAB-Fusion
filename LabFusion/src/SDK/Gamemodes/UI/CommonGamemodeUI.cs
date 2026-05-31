using Il2CppTMPro;

using LabFusion.UI.Elements;
using LabFusion.UI.Styles;

namespace LabFusion.SDK.Gamemodes;

public static class CommonGamemodeUI
{
    public static UIElement CreateRoleObjectiveUI(out LabelElement roleLabel, out LabelElement objectiveLabel)
    {
        var root = new UIElement();
        root.Style.AlignItems = Align.Center;
        root.Style.TextAlignment = TextAlignmentOptions.Center;
        root.Style.Margins = new BorderOffsets(0, 0, 5, 5);

        roleLabel = new LabelElement("Role");
        roleLabel.Style.TextAutoSize = new TextAutoSize(StyleDefaults.FontSize * 0.5f, StyleDefaults.FontSize * 2.5f);
        roleLabel.Style.FontStyle = FontStyles.Bold | FontStyles.UpperCase;
        roleLabel.Style.Padding = new BorderOffsets(10, 10, 0, 0);
        root.Add(roleLabel);

        objectiveLabel = new LabelElement("Objective");
        objectiveLabel.Style.TextAutoSize = new TextAutoSize(StyleDefaults.FontSize * 0.5f, StyleDefaults.FontSize * 0.9f);
        objectiveLabel.Style.Padding = new BorderOffsets(15, 15, 0, 0);
        root.Add(objectiveLabel);

        return root;
    }
}
