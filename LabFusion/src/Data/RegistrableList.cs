namespace LabFusion.Data;

public sealed class RegistrableList<TRegistrable>
{
    private readonly HashSet<TRegistrable> _entries = new();
    public HashSet<TRegistrable> Entries => _entries;

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
