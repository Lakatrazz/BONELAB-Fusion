using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class TypeSelector<TElement> : IStyleSelector where TElement : UIElement
{
    public Specificity Specificity { get; } = Specificity.UnitType;

    public bool Matches(UIElement element) => element is TElement;
}
