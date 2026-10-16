using Object = UnityEngine.Object;

namespace LabFusion.Utilities;

public class FusionComponentCache<TSource, TComponent> where TSource : Object where TComponent : class
{
    private readonly Dictionary<int, TComponent> _instanceIDToComponent = new();

    public ICollection<TComponent> Components => _instanceIDToComponent.Values;

    public TComponent Get(TSource source) => Get(source.GetInstanceID());

    public TComponent Get(int instanceID) => TryGet(instanceID, out var component) ? component : null;

    public bool TryGet(TSource source, out TComponent component) => TryGet(source.GetInstanceID(), out component);

    public bool TryGet(int instanceID, out TComponent component) => _instanceIDToComponent.TryGetValue(instanceID, out component);

    public bool ContainsSource(TSource source)
    {
        return _instanceIDToComponent.ContainsKey(source.GetInstanceID());
    }

    public void Add(TSource source, TComponent component) => Add(source.GetInstanceID(), component);

    public void Add(int instanceID, TComponent component)
    {
        if (_instanceIDToComponent.ContainsKey(instanceID))
        {
            _instanceIDToComponent[instanceID] = component;

#if DEBUG
            FusionLogger.Warn("Attempted to add component to a ComponentCache, but Source already existed. This is probably fine.");
#endif

            return;
        }

        _instanceIDToComponent.Add(instanceID, component);
    }

    public void Remove(TSource source) => Remove(source.GetInstanceID());

    public void Remove(int instanceID) => _instanceIDToComponent.Remove(instanceID);
}
