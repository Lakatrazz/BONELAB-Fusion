using LabFusion.Extensions;
using LabFusion.UI.Styles;

namespace LabFusion.UI.Elements;

/// <summary>
/// Base class for an element in a UI tree, based loosely on Unity's VisualElement system.
/// </summary>
public class UIElement : IRepaintNotifier
{
    private Style _style = null;
    public Style Style
    {
        get
        {
            _style ??= new(Repaint);
            return _style;
        }
    }

    public UIElement Parent => _logicalParent;

    public IReadOnlyList<UIElement> Children
    {
        get
        {
            if (ContentContainer == this)
            {
                return PhysicalChildren;
            }

            return ContentContainer.Children;
        }
    }

    public IReadOnlyList<UIElement> PhysicalChildren => _physicalChildren;

    public virtual UIElement ContentContainer => this;

    public HashSet<string> StyleClasses { get; } = new();

    public event Action Repainted;

    private readonly List<UIElement> _physicalChildren = new();

    private UIElement _physicalParent = null;

    private UIElement _logicalParent = null;

    public void Add(UIElement child)
    {
        var contentContainer = ContentContainer;

        if (contentContainer == this)
        {
            AddImmediateChild(child);
        }
        else
        {
            contentContainer.Add(child);
        }

        child._logicalParent = this;
    }
    
    public void Remove(UIElement child)
    {
        var contentContainer = ContentContainer;

        if (contentContainer == this)
        {
            RemoveImmediateChild(child);
        }
        else
        {
            contentContainer.Remove(child);
        }
    }

    public void AddStyleClass(string className) => StyleClasses.Add(className);

    public void RemoveStyleClass(string className) => StyleClasses.Remove(className);

    public bool HasStyleClass(string className) => StyleClasses.Contains(className);

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

    protected void AddImmediateChild(UIElement child)
    {
        child._physicalParent = this;
        child._logicalParent = this;

        _physicalChildren.Add(child);
    }

    protected void RemoveImmediateChild(UIElement child)
    {
        if (child.Parent != this)
        {
            return;
        }

        _physicalChildren.Remove(child);

        child._physicalParent = null;
        child._logicalParent = null;
    }
}
