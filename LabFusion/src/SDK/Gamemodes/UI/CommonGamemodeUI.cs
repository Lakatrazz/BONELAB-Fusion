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

        roleLabel = new LabelElement("Role");
        roleLabel.Style.FontSize = Length.FromRatio(2.5f);
        roleLabel.Style.Margins = new BorderOffsets(0, 0, 5, 0);
        roleLabel.Style.FontStyle = FontStyles.Bold | FontStyles.UpperCase;
        root.Add(roleLabel);

        objectiveLabel = new LabelElement("Objective");
        objectiveLabel.Style.FontSize = Length.FromRatio(0.9f);
        root.Add(objectiveLabel);

        return root;
    }
}
