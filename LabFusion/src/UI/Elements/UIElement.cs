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

    private Style _resolvedStyle = null;
    public IReadOnlyStyle ResolvedStyle => _resolvedStyle;

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

    public List<StyleSheet> StyleSheets { get; } = new();

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

    public void AddStyleSheet(StyleSheet styleSheet) => StyleSheets.Add(styleSheet);

    public void RemoveStyleSheet(StyleSheet styleSheet) => StyleSheets.Remove(styleSheet);

    public void ClearStyleSheets() => StyleSheets.Clear();

    public void Resolve() => Resolve(CollectStyleSheets());

    public void Resolve(List<StyleSheet> styleSheets)
    {
        var resolvedStyle = new Style(Style);

        List<StyleRule> matchingRules = new();

        foreach (var styleSheet in styleSheets)
        {
            matchingRules.AddRange(styleSheet.GetMatchingRules(this));
        }

        var orderedRules = matchingRules.OrderBy(rule => rule.Selector.Specificity);

        foreach (var rule in orderedRules)
        {
            rule.ApplyRule(Style, resolvedStyle);
        }

        _resolvedStyle = resolvedStyle;

        // Apply to children
        foreach (var child in PhysicalChildren)
        {
            var childStyleSheets = styleSheets;

            child.Resolve(childStyleSheets);
        }
    }

    public List<StyleSheet> CollectStyleSheets()
    {
        List<StyleSheet> collectedStyleSheets = new();

        collectedStyleSheets.AddRange(StyleSheets);

        var parent = Parent;

        while (parent != null)
        {
            var parentStyleSheets = parent.StyleSheets;

            if (parentStyleSheets.Count > 0)
            {
                collectedStyleSheets.AddRange(parentStyleSheets);
            }

            parent = parent.Parent;
        }

        // Child style sheets should be more specific
        // Since we went up the hierarchy, the order needs to be reversed for correct specificity
        collectedStyleSheets.Reverse();

        return collectedStyleSheets;
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
