namespace LabFusion.UI.Styles;

public class StyleRule
{
    public IStyleSelector Selector { get; }

    public Style Declaration { get; }

    public StyleRule(IStyleSelector selector, Style declaration)
    {
        Selector = selector;
        Declaration = declaration;
    }

    public void ApplyRule(Style originalStyle, Style resolvedStyle)
    {
        foreach (var propertyPair in Declaration.SetProperties)
        {
            var propertyName = propertyPair.Key;

            if (originalStyle.HasSetProperty(propertyName))
            {
                continue;
            }

            var propertyValue = propertyPair.Value;

            resolvedStyle.SetProperty(propertyName, propertyValue);
        }
    }
}
