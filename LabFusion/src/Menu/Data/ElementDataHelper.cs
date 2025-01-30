using LabFusion.Marrow.Proxies;
using LabFusion.Utilities;

namespace LabFusion.Menu.Data;

public static class ElementDataHelper
{
    public static void ApplyIntData(IntElement element, IntElementData data)
    {
        element.Title = data.Title;
        element.Increment = data.Increment;
        element.MinValue = data.MinValue;
        element.MaxValue = data.MaxValue;
        element.Value = data.Value;

        element.OnValueChanged = (v) => data.Value = v;
    }

    public static void ApplyFloatData(FloatElement element, FloatElementData data)
    {
        element.Title = data.Title;
        element.Increment = data.Increment;
        element.MinValue = data.MinValue;
        element.MaxValue = data.MaxValue;
        element.Value = data.Value;

        element.OnValueChanged = (v) => data.Value = v;
    }

    public static void ApplyBoolData(BoolElement element, BoolElementData data)
    {
        element.Title = data.Title;
        element.Value = data.Value;

        element.OnValueChanged = (v) => data.Value = v;
    }

    public static void ApplyStringData(StringElement element, StringElementData data)
    {
        element.Title = data.Title;
        element.Value = data.Value;

        element.OnValueChanged = (v) => data.Value = v;
    }

    public static void ApplyFunctionData(FunctionElement element, FunctionElementData data)
    {
        element.Title = data.Title;
        element.OnPressed = data.OnPressed;
    }

    public static void ApplyEnumData(EnumElement element, EnumElementData data)
    {
        element.Title = data.Title;
        element.Value = data.Value;
        element.EnumType = data.EnumType;
        element.OnValueChanged = (v) => data.Value = v;
    }

    public static void ApplyGroupData(GroupElement group, GroupElementData data)
    {
        foreach (var elementData in data.Elements)
        {
            try
            {
                AddElementToGroup(group, elementData);
            }
            catch (Exception e)
            {
                FusionLogger.LogException($"adding ElementData {elementData.Title}", e);
            }
        }
    }

    private static void AddElementToGroup(GroupElement group, ElementData data)
    {
        if (data is GroupElementData groupData)
        {
            var groupElement = group.AddElement<GroupElement>(groupData.Title);
            ApplyGroupData(groupElement, groupData);
        }

        if (data is IntElementData intData)
        {
            var intElement = group.AddElement<IntElement>(intData.Title);
            ApplyIntData(intElement, intData);
        }

        if (data is FloatElementData floatData)
        {
            var floatElement = group.AddElement<FloatElement>(floatData.Title);
            ApplyFloatData(floatElement, floatData);
        }

        if (data is BoolElementData boolData)
        {
            var boolElement = group.AddElement<BoolElement>(boolData.Title);
            ApplyBoolData(boolElement, boolData);
        }

        if (data is StringElementData stringData)
        {
            var stringElement = group.AddElement<StringElement>(stringData.Title);
            ApplyStringData(stringElement, stringData);
        }

        if (data is FunctionElementData functionData)
        {
            var functionElement = group.AddElement<FunctionElement>(functionData.Title);
            ApplyFunctionData(functionElement, functionData);
        }

        if (data is EnumElementData enumData)
        {
            var enumElement = group.AddElement<EnumElement>(enumData.Title);
            ApplyEnumData(enumElement, enumData);
        }
    }
}