using LabFusion.UI.Elements;

namespace LabFusion.UI.Styles;

public class StyleSheet
{
    public List<StyleRule> Rules { get; } = new();

    public List<StyleRule> GetMatchingRules(UIElement element)
    {
        List<StyleRule> matchingRules = new();

        foreach (var rule in Rules)
        {
            if (rule.Selector.Matches(element))
            {
                matchingRules.Add(rule);
            }
        }

        return matchingRules;
    }
}
