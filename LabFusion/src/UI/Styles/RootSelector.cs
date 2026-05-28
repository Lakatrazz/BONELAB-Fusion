using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class RootSelector : IStyleSelector
{
    public Specificity Specificity => Specificity.UnitClass;

    public bool Matches(UIElement element) => element.Parent == null;
}
