namespace MarrowFusion.Epic;

internal abstract class EOSInterface
{
    internal virtual Task<bool> InitializeAsync()
    {
        return Task.FromResult(true);
    }

    internal virtual void Tick() { }

    internal virtual void Shutdown() { }
}