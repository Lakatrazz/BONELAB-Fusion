using Il2CppSLZ.Marrow.Pool;

using LabFusion.Entities;
using LabFusion.Marrow.Integration;

using MelonLoader;

using UnityEngine;

namespace LabFusion.SDK.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class DroppedItem : MonoBehaviour
{
    public DroppedItem(IntPtr intPtr) : base(intPtr) { }

    private NetworkEntity _entity = null;
    private Poolee _poolee = null;

    private bool _isDespawned = false;

    public void Initialize(NetworkEntity entity, Poolee poolee)
    {
        _entity = entity;
        _poolee = poolee;

        GamemodeDropper.DroppedItems.Add(poolee);

        poolee.OnDespawnDelegate += (Action<GameObject>)OnDespawn;
    }

    private void OnDespawn(GameObject gameObject)
    {
        if (_isDespawned)
        {
            return;
        }

        _isDespawned = true;

        GamemodeDropper.DroppedItems.Remove(_poolee);

        _entity = null;
        _poolee = null;

        Destroy(this);
    }

    private void OnDestroy()
    {
        OnDespawn(null);
    }
}
