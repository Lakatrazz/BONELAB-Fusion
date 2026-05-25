using LabFusion.Marrow.Integration;
using LabFusion.UI.Elements;

using UnityEngine;

namespace LabFusion.UI;

public static class UIElementDrawer
{
    public static void DrawUITree(UIElement rootElement, UIElementView rootView)
    {
        rootView.RemoveChildren();

        rootView.AssignElement(rootElement);

        foreach (var child in rootElement.Children)
        {
            DrawUIChild(child, rootView);
        }
    }

    private static void DrawUIChild(UIElement childElement, UIElementView parentView)
    {
        var childElementView = CreateElementView(childElement, parentView.Container);

        parentView.AddChild(childElementView);

        foreach (var child in childElement.Children)
        {
            DrawUIChild(child, childElementView);
        }
    }

    private static UIElementView CreateElementView(UIElement element, Transform parent)
    {
        var spawner = UIElementSpawner.Instance;

        if (element is TextElement textElement)
        {
            var textElementView = spawner.CreateElementView<TextElementView>(parent);

            textElementView.AssignElement(textElement);

            return textElementView;
        }

        var elementView = spawner.CreateElementView<UIElementView>(parent);

        elementView.AssignElement(element);

        return elementView;
    }
}
