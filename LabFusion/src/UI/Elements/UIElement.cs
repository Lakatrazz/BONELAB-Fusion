using LabFusion.Extensions;
using LabFusion.UI.Resources;
using LabFusion.UI.Styles;

namespace LabFusion.UI.Elements;

/// <summary>
/// Base class for an element in a UI tree, based loosely on Unity's VisualElement system.
/// </summary>
public class UIElement
{
    private Style _style = null;
    public Style Style
    {
        get
        {
            _style ??= new(MarkStyleDirty);
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

    public IReadOnlyList<StyleSheet> StyleSheets => _styleSheets;
    public IReadOnlyList<StyleSheet> ExternalStyleSheets => _externalStyleSheets;
    public IReadOnlyList<StyleSheet> ResolvedStyleSheets => _resolvedStyleSheets;

    public bool IsContentDirty { get; private set; } = false;

    public bool IsStyleDirty { get; private set; } = false;

    public bool IsChildrenDirty { get; private set; } = false;

    public event Action ContentGenerated, ChildrenGenerated, StyleResolved;

    private readonly List<UIElement> _physicalChildren = new();

    private UIElement _physicalParent = null;
    private UIElement _logicalParent = null;

    private readonly List<StyleSheet> _styleSheets = new();
    private List<StyleSheet> _externalStyleSheets = null;
    private List<StyleSheet> _resolvedStyleSheets = new();

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

    public void AddStyleClass(string className)
    {
        StyleClasses.Add(className);
        MarkStyleDirty();
    }

    public void RemoveStyleClass(string className)
    {
        StyleClasses.Remove(className);
        MarkStyleDirty();
    }

    public bool HasStyleClass(string className) => StyleClasses.Contains(className);

    public void AddStyleSheet(StyleSheet styleSheet)
    {
        _styleSheets.Add(styleSheet);
        MarkStyleDirty();
    }

    public void RemoveStyleSheet(StyleSheet styleSheet)
    {
        _styleSheets.Remove(styleSheet);
        MarkStyleDirty();
    }

    public void ClearStyleSheets()
    {
        _styleSheets.Clear();
        MarkStyleDirty();
    }

    public void SetExternalStyleSheets(List<StyleSheet> externalStyleSheets)
    {
        _externalStyleSheets = externalStyleSheets;
        MarkStyleDirty();
    }

    public void ClearExternalStyleSheets() => SetExternalStyleSheets(null);

    public void ResolveStyle()
    {
        ResolveStyleSheets();

        var resolvedStyle = new Style(Style);

        InheritProperties(Style, resolvedStyle);

        var uniqueSheets = ResolvedStyleSheets.Distinct();

        List<StyleRule> matchingRules = new();

        foreach (var styleSheet in uniqueSheets)
        {
            matchingRules.AddRange(styleSheet.GetMatchingRules(this));
        }

        var orderedRules = matchingRules.OrderBy(rule => rule.Selector.Specificity);

        foreach (var rule in orderedRules)
        {
            rule.ApplyRule(Style, resolvedStyle);
        }

        ProcessProperties(resolvedStyle);

        _resolvedStyle = resolvedStyle;

        // Notify listeners that the style has been resolved
        StyleResolved?.InvokeSafe("executing StyleResolved event");

        IsStyleDirty = false;

        // Resolve the styles for all attached children
        foreach (var child in PhysicalChildren)
        {
            child.ResolveStyle();
        }
    }

    public void MarkContentDirty()
    {
        IsContentDirty = true;
    }

    public void MarkStyleDirty()
    {
        IsStyleDirty = true;
    }

    public void MarkChildrenDirty()
    {
        IsChildrenDirty = true;
    }

    public void GenerateContent()
    {
        ContentGenerated?.InvokeSafe("executing ContentGenerated event");

        IsContentDirty = false;
    }

    public void GenerateChildren()
    {
        ChildrenGenerated?.InvokeSafe("executing ChildrenGenerated event");

        IsChildrenDirty = false;
    }

    protected void AddImmediateChild(UIElement child)
    {
        child._physicalParent = this;
        child._logicalParent = this;

        _physicalChildren.Add(child);

        MarkChildrenDirty();
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

        MarkChildrenDirty();
    }

    private void ResolveStyleSheets()
    {
        var resolvedStyleSheets = new List<StyleSheet>();

        if (Parent != null)
        {
            resolvedStyleSheets.AddRange(Parent.ResolvedStyleSheets);
        }

        if (ExternalStyleSheets != null)
        {
            resolvedStyleSheets.AddRange(ExternalStyleSheets);
        }

        resolvedStyleSheets.AddRange(StyleSheets);

        _resolvedStyleSheets = resolvedStyleSheets.Distinct().ToList();
    }

    private void InheritProperties(Style originalStyle, Style resolvedStyle)
    {
        var parent = Parent;

        if (parent == null)
        {
            return;
        }

        var parentStyle = parent.ResolvedStyle;

        if (parentStyle == null)
        {
            return;
        }

        foreach (var inheritedProperty in CommonStyleProperties.InheritedProperties)
        {
            if (originalStyle.HasSetProperty(inheritedProperty))
            {
                continue;
            }

            if (parentStyle.SetProperties.TryGetValue(inheritedProperty, out var inheritedValue))
            {
                resolvedStyle.SetProperty(inheritedProperty, inheritedValue);
            }
        }
    }

    private void ProcessProperties(Style resolvedStyle)
    {
        foreach (var propertyPair in resolvedStyle.SetProperties)
        {
            var propertyName = propertyPair.Key;
            var propertyValue = propertyPair.Value;

            ProcessProperty(resolvedStyle, propertyName, propertyValue);
        }
    }

    private void ProcessProperty(Style resolvedStyle, string propertyName, IStyleValue propertyValue)
    {
        // Convert non-pixel lengths to pixels
        if (propertyValue is StyleValue<Length> styleLength)
        {
            ProcessLength(resolvedStyle, propertyName, styleLength);
        }
    }

    private void ProcessLength(Style resolvedStyle, string propertyName, StyleValue<Length> styleLength)
    {
        var originalLength = styleLength.Value;

        if (originalLength.Unit == LengthUnit.Pixel)
        {
            return;
        }

        float inheritedPixels = StyleDefaults.GetDefaultLength(propertyName);

        if (Parent.ResolvedStyle.SetProperties.TryGetValue(propertyName, out var inheritedValue) && inheritedValue is StyleValue<Length> inheritedLength)
        {
            inheritedPixels = inheritedLength.Value;
        }

        var resolvedLength = originalLength.ToPixels(inheritedPixels);

        resolvedStyle.SetProperty(propertyName, new StyleValue<Length>(resolvedLength));
    }
}
