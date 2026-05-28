using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class ChildSelector : IStyleSelector
{
    public IStyleSelector Parent { get; }
    public IStyleSelector Child { get; }

    public Specificity Specificity => Parent.Specificity + Child.Specificity;

    public ChildSelector(IStyleSelector parent, IStyleSelector child)
    {
        Parent = parent;
        Child = child;
    }

    public bool Matches(UIElement element)
    {
        if (!Child.Matches(element))
        {
            return false;
        }

        var parentElement = element.Parent;

        return parentElement != null && Parent.Matches(parentElement);
    }
}
