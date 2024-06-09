using System.Xml.Linq;

namespace LabFusion.Extensions
{
    public static class XElementExtensions
    {
        public static bool TryGetAttribute(this XElement element, string tag, out string value, string fallback = "")
        {
            var attribute = element.Attribute(tag);

            value = fallback;
            if (attribute != null)
                value = attribute.Value;

            return attribute != null;
        }
    }
}
