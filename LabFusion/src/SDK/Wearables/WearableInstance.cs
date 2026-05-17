using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow.Integration;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WearableInstance
{
    public bool HiddenInView { get; set; } = false;

    private bool _isShown = true;
    public bool IsShown
    {
        get
        {
            return _isShown;
        }
        set
        {
            _isShown = value;

            ApplyVisibility();
        }
    }

    private bool _isPrimaryShown = true;
    public bool IsPrimaryShown
    {
        get
        {
            return _isPrimaryShown;
        }
        set
        {
            _isPrimaryShown = value;

            ApplyMainVisibility();
        }
    }

    private bool _isReflectionShown = true;
    public bool IsReflectionShown
    {
        get
        {
            return _isReflectionShown;
        }
        set
        {
            _isReflectionShown = value;

            ApplyReflectionVisibility();
        }
    }

    public WearablePoint Point { get; set; } = WearablePoint.Head;

    public Vector3 Position { get; private set; } = Vector3.zero;
    public Quaternion Rotation { get; private set; } = Quaternion.identity;
    public Vector3 Scale { get; private set; } = Vector3.one;

    public Transform MainInstance { get; private set; } = null;

    public List<Transform> ReflectionInstances { get; } = new();

    private int _reflectionCount = 0;
    public int ReflectionCount
    {
        get
        {
            return _reflectionCount;
        }
        set
        {
            if (_reflectionCount == value)
            {
                return;
            }

            _reflectionCount = value;

            SpawnReflections(value);
        }
    }

    public SpawnableCrateReference SpawnableCrateReference { get; set; } = new();

    public void Spawn()
    {
        SpawnMainInstance();

        SpawnReflections(ReflectionCount);
    }

    public void Despawn()
    {
        DespawnMainInstance();

        DespawnReflections();
    }

    public void Destroy()
    {
        Despawn();
    }

    public void UpdateMain(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;

        ApplyMainTransform();
    }

    public void UpdateReflection(Transform reflectionOrigin, int reflectionIndex)
    {
        if (ReflectionInstances.Count <= reflectionIndex)
        {
            return;
        }

        // Reflect the wearable's rotation
        Vector3 forward = Rotation * Vector3.forward;
        Vector3 up = Rotation * Vector3.up;

        Vector3 reflectionAxis = reflectionOrigin.forward;

        var reflectedForward = Vector3.Reflect(forward, reflectionAxis);
        var reflectedUp = Vector3.Reflect(up, reflectionAxis);

        Quaternion reflectedRotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // Reflect the wearable's position
        var reflectionPivot = reflectionOrigin.position;

        Vector3 reflectedPosition = Position - reflectionPivot;
        reflectedPosition = Vector3.Reflect(reflectedPosition, reflectionAxis);
        reflectedPosition += reflectionPivot;

        // Apply the reflected transform
        var reflectionTransform = ReflectionInstances[reflectionIndex];

        reflectionTransform.SetPositionAndRotation(reflectedPosition, reflectedRotation);
        reflectionTransform.localScale = Scale;
    }

    private void ApplyMainTransform()
    {
        if (MainInstance == null)
        {
            return;
        }

        MainInstance.SetPositionAndRotation(Position, Rotation);
        MainInstance.localScale = Scale;
    }

    private void ApplyVisibility()
    {
        ApplyMainVisibility();
        ApplyReflectionVisibility();
    }

    private void ApplyMainVisibility()
    {
        if (MainInstance == null)
        {
            return;
        }

        var mainGameObject = MainInstance.gameObject;
        mainGameObject.SetActive(IsShown && IsPrimaryShown);
    }

    private void ApplyReflectionVisibility()
    {
        bool active = IsShown && IsReflectionShown;

        foreach (var reflection in ReflectionInstances)
        {
            var reflectionGameObject = reflection.gameObject;
            reflectionGameObject.SetActive(active);
        }
    }

    private void SpawnInstance(Action<GameObject> onSpawned)
    {
        var onLoaded = (GameObject go) =>
        {
            var instance = GameObject.Instantiate(go);

            instance.name = go.name;

            onSpawned?.Invoke(instance);
        };
        SpawnableCrateReference.Crate.LoadAsset(onLoaded);
    }

    private void SpawnMainInstance()
    {
        SpawnInstance((instance) =>
        {
            MainInstance = instance.transform;

            instance.SetActive(IsShown && IsPrimaryShown);

            ApplyMainTransform();
        });
    }

    private void SpawnReflections(int reflectionCount)
    {
        ReflectionInstances.RemoveAll(t => t == null);

        if (reflectionCount > ReflectionInstances.Count)
        {
            for (var i = 0; i < reflectionCount - ReflectionInstances.Count; i++)
            {
                var instanceIndex = i;

                SpawnInstance((instance) =>
                {
                    instance.name = $"{instance.name} (Reflection ({instanceIndex}))";

                    instance.SetActive(IsShown && IsReflectionShown);

                    ReflectionInstances.Add(instance.transform);
                });
            }
        }
        else if (reflectionCount < ReflectionInstances.Count)
        {
            for (var i = reflectionCount - 1; i < ReflectionInstances.Count; i++)
            {
                var reflection = ReflectionInstances[i];

                if (reflection == null)
                {
                    continue;
                }

                GameObject.Destroy(reflection);
            }

            ReflectionInstances.RemoveRange(reflectionCount - 1, ReflectionInstances.Count - reflectionCount);
        }
    }

    private void DespawnMainInstance()
    {
        if (MainInstance == null)
        {
            return;
        }

        GameObject.Destroy(MainInstance.gameObject);
        MainInstance = null;
    }

    private void DespawnReflections()
    {
        foreach (var reflection in ReflectionInstances)
        {
            if (reflection == null)
            {
                continue;
            }

            GameObject.Destroy(reflection.gameObject);
        }

        ReflectionInstances.Clear();
    }
}
