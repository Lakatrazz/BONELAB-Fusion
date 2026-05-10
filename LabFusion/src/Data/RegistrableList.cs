namespace LabFusion.Data;

public sealed class RegistrableList<TRegistrable>
{
    private readonly List<TRegistrable> _entries = new();
    public List<TRegistrable> Entries => _entries;

    public void Register(TRegistrable entity)
    {
        if (_entries.Contains(entity))
        {
            return;
        }

        _entries.Add(entity);
    }

    public void Unregister(TRegistrable entity)
    {
        _entries.Remove(entity);
    }
}
