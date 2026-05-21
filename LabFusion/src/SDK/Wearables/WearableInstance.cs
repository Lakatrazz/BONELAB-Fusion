using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;

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

    public AvatarAnchor Anchor { get; set; } = new(AvatarPoint.Head, AvatarAlignment.Center, AvatarSide.Center);

    public Vector3 Position { get; private set; } = Vector3.zero;
    public Quaternion Rotation { get; private set; } = Quaternion.identity;
    public Vector3 Scale { get; private set; } = Vector3.one;

    public RigManager RigManager { get; set; } = null;

    public Transform MainInstance { get; private set; } = null;

    public bool HasMainInstance => MainInstance != null;

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

    public List<IWearableComponent> Components { get; set; } = new();

    private bool _dontUpdateReflectionsThisFrame = false;

    public void Initialize(bool local, PlayerID playerID = null)
    {
        foreach (var component in Components)
        {
            component.OnInitialize(local, playerID);
        }
    }

    public void Spawn(RigManager rigManager)
    {
        RigManager = rigManager;

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

        foreach (var component in Components)
        {
            component.OnDeinitialize();
        }
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

        if (_dontUpdateReflectionsThisFrame)
        {
            return;
        }

        // Reflect the wearable's rotation
        Vector3 forward = Rotation * Vector3.forward;
        Vector3 up = Rotation * Vector3.up;

        Vector3 reflectionAxis = reflectionOrigin.forward;

        var reflectedForward = Vector3.Reflect(forward, reflectionAxis);
        var reflectedUp = Vector3.Reflect(up, reflectionAxis);

        Quaternion reflectedRotation = Quaternion.LookRotation(-reflectedForward, reflectedUp);

        // Reflect the wearable's position
        var reflectionPivot = reflectionOrigin.position;

        Vector3 reflectedPosition = Position - reflectionPivot;
        reflectedPosition = Vector3.Reflect(reflectedPosition, reflectionAxis);
        reflectedPosition += reflectionPivot;

        // Apply the reflected transform
        var reflectionTransform = ReflectionInstances[reflectionIndex];

        // Reflection origins have negative scale, the reflection is parented so that the models get properly reflected
        reflectionTransform.parent = reflectionOrigin;

        reflectionTransform.SetPositionAndRotation(reflectedPosition, reflectedRotation);
        reflectionTransform.localScale = Scale;
    }

    public void Tick(float deltaTime)
    {
        _dontUpdateReflectionsThisFrame = false;

        if (!HasMainInstance)
        {
            return;
        }

        foreach (var component in Components)
        {
            component.OnTick(deltaTime);
        }
    }

    private void ApplyMainTransform()
    {
        if (!HasMainInstance)
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

    private void SpawnInstance(Action<GameObject> onSpawned, Transform parent = null)
    {
        var onLoaded = (GameObject go) =>
        {
            var instance = GameObject.Instantiate(go, parent);

            instance.name = go.name;

            onSpawned?.Invoke(instance);
        };
        SpawnableCrateReference.Crate.LoadAsset(onLoaded);
    }

    private void SpawnMainInstance()
    {
        SpawnInstance(OnMainInstanceCreated);
    }

    private void OnMainInstanceCreated(GameObject mainInstance)
    {
        MainInstance = mainInstance.transform;

        mainInstance.SetActive(IsShown && IsPrimaryShown);

        ApplyMainTransform();

        foreach (var component in Components)
        {
            component.OnMainInstanceCreated(mainInstance, RigManager);
        }
    }

    private void SpawnReflections(int reflectionCount)
    {
        DespawnReflections();

        if (reflectionCount <= 0)
        {
            return;
        }

        // Reflections are parented to the disabled container until their transform is updated
        // This prevents them from being visible when they shouldn't be
        // The transform update automatically reparents them to the reflection origin
        var parent = DisabledContainer.ContainerTransform;

        for (var i = 0; i < reflectionCount; i++)
        {
            var instanceIndex = i;

            SpawnInstance((instance) =>
            {
                instance.name = $"{instance.name} (Reflection ({instanceIndex}))";

                instance.SetActive(IsShown && IsReflectionShown);

                ReflectionInstances.Add(instance.transform);
            }, parent);
        }

        // Reflection updates are delayed by one frame to prevent pop-in from the reflection scaling
        // For whatever reason, the reflection origin isn't always scaled, and changes scales between frames
        _dontUpdateReflectionsThisFrame = true;
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
