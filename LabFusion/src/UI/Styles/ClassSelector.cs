using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class ClassSelector : IStyleSelector
{
    public string ClassName { get; }

    public Specificity Specificity { get; } = Specificity.UnitClass;

    public ClassSelector(string className)
    {
        ClassName = className;
    }

    public bool Matches(UIElement element) => element.HasStyleClass(ClassName);
}
