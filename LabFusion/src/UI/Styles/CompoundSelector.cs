using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class CompoundSelector : IStyleSelector
{
    public IStyleSelector Left { get; }
    public IStyleSelector Right { get; }

    public Specificity Specificity => Left.Specificity + Right.Specificity;

    public CompoundSelector(IStyleSelector left, IStyleSelector right)
    {
        Left = left; 
        Right = right;
    }

    public bool Matches(UIElement element) => Left.Matches(element) && Right.Matches(element);
}
