using LabFusion.Extensions;

using Object = UnityEngine.Object;

namespace LabFusion.Utilities;

public class FusionComponentCache<TSource, TComponent> where TSource : Object where TComponent : class
{
    private readonly Dictionary<TSource, TComponent> _cache = new(new UnityComparer());

    public ICollection<TComponent> Components => _cache.Values;

    public TComponent Get(TSource source)
    {
        if (_cache.TryGetValue(source, out var component))
        {
            return component;
        }

        return null;
    }

    public bool TryGet(TSource source, out TComponent value)
    {
        return _cache.TryGetValue(source, out value);
    }

    public bool ContainsSource(TSource source)
    {
        return _cache.ContainsKey(source);
    }

    public void Add(TSource source, TComponent component)
    {
        if (_cache.ContainsKey(source))
        {
            _cache[source] = component;

#if DEBUG
            FusionLogger.Warn("Attempted to add component to a ComponentCache, but Source already existed. This is probably fine.");
#endif

            return;
        }

        _cache.Add(source, component);
    }

    public void Remove(TSource source)
    {
        _cache.Remove(source);
    }
}
