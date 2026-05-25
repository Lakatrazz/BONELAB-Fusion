using LabFusion.Extensions;

namespace LabFusion.UI.Elements;

/// <summary>
/// Base class for an element in a UI tree, based loosely on Unity's VisualElement system.
/// </summary>
public class UIElement : IRepaintNotifier
{
    private UIStyle _style = null;
    public UIStyle Style
    {
        get
        {
            _style ??= new(Repaint);
            return _style;
        }
    }

    private UIElement _parent = null;
    public UIElement Parent
    {
        get => _parent;
        set
        {
            if (_parent == value)
            {
                return;
            }

            _parent?.Children.Remove(this);

            _parent = value;

            value?.Children?.Add(this);
        }
    }

    public List<UIElement> Children { get; } = new();

    public event Action Repainted;

    public void Add(UIElement child)
    {
        child.Parent = this;
    }
    
    public void Remove(UIElement child)
    {
        if (child.Parent != this)
        {
            return;
        }

        child.Parent = null;
    }

    public List<TElement> Query<TElement>() where TElement : UIElement
    {
        List<TElement> result = new();

        foreach (var child in Children)
        {
            if (child is TElement element)
            {
                result.Add(element);
            }

            var childQuery = child.Query<TElement>();

            if (childQuery.Count > 0)
            {
                result.AddRange(childQuery);
            }
        }

        return result;
    }

    public void Repaint()
    {
        Repainted?.InvokeSafe("executing Repainted event");
    }
}
