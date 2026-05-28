using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class UniversalSelector : IStyleSelector
{
    public Specificity Specificity { get; } = Specificity.Identity;

    public bool Matches(UIElement element) => true;
}
