namespace LabFusion.Menu.Data;

public class GroupElementData : ElementData
{
    private List<ElementData> _elements = new();
    public List<ElementData> Elements => _elements;

    public GroupElementData(string title)
    {
        Title = title;
    }

    public GroupElementData() { }

    public void AddElement<TElement>(TElement element) where TElement : ElementData
    {
        _elements.Add(element);
    }
}
