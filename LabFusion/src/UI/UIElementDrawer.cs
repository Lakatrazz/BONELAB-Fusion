using LabFusion.Marrow.Integration;
using LabFusion.UI.Elements;

namespace LabFusion.UI;

public static class UIElementDrawer
{
    public static void DrawUITree(UIElement rootElement, UIElementView rootView)
    {
        rootView.AssignElement(rootElement);
    }
}
