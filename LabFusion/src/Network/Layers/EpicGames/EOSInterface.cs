using System.Collections;

namespace LabFusion.Network;

internal abstract class EOSInterface
{
    internal virtual IEnumerator InitializeAsync(Action<bool> onComplete)
    {
        onComplete?.Invoke(true);
        yield return null;
    }
    
    internal virtual void Shutdown() { }
}