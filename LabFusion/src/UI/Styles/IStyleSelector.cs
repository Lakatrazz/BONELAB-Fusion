using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public interface IStyleSelector
{
    Specificity Specificity { get; }

    bool Matches(UIElement element);
}
