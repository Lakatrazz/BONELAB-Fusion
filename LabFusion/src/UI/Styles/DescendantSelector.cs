using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class DescendantSelector : IStyleSelector
{
    public IStyleSelector Parent { get; }
    public IStyleSelector Descendant { get; }

    public Specificity Specificity => Parent.Specificity + Descendant.Specificity;

    public DescendantSelector(IStyleSelector parent, IStyleSelector descendant)
    {
        Parent = parent;
        Descendant = descendant;
    }

    public bool Matches(UIElement element)
    {
        if (!Descendant.Matches(element))
        {
            return false;
        }

        var parentElement = element.Parent;

        while (parentElement != null)
        {
            if (Parent.Matches(parentElement)) 
            {
                return true;
            }

            parentElement = parentElement.Parent;
        }

        return false;
    }
}
