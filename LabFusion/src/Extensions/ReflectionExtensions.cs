using System.Reflection;
using System.Text;

namespace LabFusion.Extensions;

public static class ReflectionExtensions
{
    public static string GetNameWithParameters(this MethodInfo method)
    {
        var builder = new StringBuilder(method.Name);

        var parameters = method.GetParameters();

        bool first = true;

        foreach (var parameter in parameters)
        {
            builder.Append(first ? ": " : ", ");
            builder.Append(parameter.ParameterType.FullName);

            first = false;
        }

        return builder.ToString();
    }
}
