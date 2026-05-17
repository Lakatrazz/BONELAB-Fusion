using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow.Integration;

using UnityEngine;

namespace LabFusion.SDK.Wearables;

public class WearableInstance
{
    public bool HiddenInView { get; set; } = false;

    public bool IsHidden { get; set; } = false;

    public WearablePoint Point { get; set; } = WearablePoint.Head;

    public Vector3 Position { get; private set; } = Vector3.zero;
    public Quaternion Rotation { get; private set; } = Quaternion.identity;
    public Vector3 Scale { get; private set; } = Vector3.one;

    public Transform Transform { get; private set; } = null;

    public List<Transform> Reflections { get; } = new();

    public SpawnableCrateReference SpawnableCrateReference { get; set; } = new();

    public void Destroy()
    {
        DestroyInstance();

        DestroyReflections();
    }

    public void CreateInstance()
    {
        SpawnInstance((instance) =>
        {
            Transform = instance.transform;

            ApplyTransform();
        });
    }

    public void CreateReflections(int reflectionCount)
    {
        Reflections.RemoveAll(t => t == null);

        if (reflectionCount > Reflections.Count)
        {
            for (var i = 0; i < reflectionCount - Reflections.Count; i++)
            {
                var instanceIndex = i;

                SpawnInstance((instance) =>
                {
                    instance.name = $"{instance.name} (Reflection ({instanceIndex}))";

                    Reflections.Add(instance.transform);
                });
            }
        }
        else if (reflectionCount < Reflections.Count)
        {
            for (var i = reflectionCount - 1; i < Reflections.Count; i++)
            {
                var reflection = Reflections[i];

                if (reflection == null)
                {
                    continue;
                }

                GameObject.Destroy(reflection);
            }

            Reflections.RemoveRange(reflectionCount - 1, Reflections.Count - reflectionCount);
        }
    }

    public void UpdateWearable(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;

        ApplyTransform();
    }

    public void UpdateReflection(Transform reflectionOrigin, int reflectionIndex)
    {
        if (Reflections.Count <= reflectionIndex)
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
        var reflectionTransform = Reflections[reflectionIndex];

        reflectionTransform.SetPositionAndRotation(reflectedPosition, reflectedRotation);
        reflectionTransform.localScale = Scale;
    }

    private void ApplyTransform()
    {
        if (Transform == null)
        {
            return;
        }

        Transform.SetPositionAndRotation(Position, Rotation);
        Transform.localScale = Scale;
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

    private void DestroyInstance()
    {
        if (Transform == null)
        {
            return;
        }

        GameObject.Destroy(Transform.gameObject);
        Transform = null;
    }

    private void DestroyReflections()
    {
        foreach (var reflection in Reflections)
        {
            if (reflection == null)
            {
                continue;
            }

            GameObject.Destroy(reflection.gameObject);
        }

        Reflections.Clear();
    }
}
