using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.UI;
using LabFusion.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Entities;

public class RigHeadUI
{
    public const float OffsetHeight = 0.2f;

    private Poolee _layoutPoolee;
    private Transform _layoutTransform;

    public bool Spawned => _layoutPoolee != null;

    private bool _visible = true;
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;

            if (_layoutPoolee == null)
            {
                return;
            }

            _layoutPoolee.gameObject.SetActive(value);
        }
    }

    private readonly HashSet<IPopupLayoutElement> _elements = new();

    public void Spawn()
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.HeadLayoutReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, OnSpawned);
    }

    public void Despawn()
    {
        if (_layoutPoolee == null)
        {
            return;
        }

        _layoutPoolee.Despawn();

        _layoutPoolee = null;
        _layoutTransform = null;

        DespawnElements();
    }

    public void RegisterElement(IPopupLayoutElement element)
    {
        bool added = _elements.Add(element);

        if (!added)
        {
            return;
        }

        if (!Spawned)
        {
            return;
        }

        // Spawn the element, then resort based on priority
        element.Spawn(_layoutTransform);

        SortElements();
    }

    public void UnregisterElement(IPopupLayoutElement element)
    {
        bool removed = _elements.Remove(element);

        if (!removed)
        {
            return;
        }

        // We can despawn the element and don't need to resort
        // This is because the sorted order without the element would be the same
        element.Despawn();
    }

    private void SortElements()
    {
        var prioritySorted = _elements.OrderBy(e => e.Priority);

        for (var i = 0; i < prioritySorted.Count(); i++)
        {
            var element = prioritySorted.ElementAt(i);

            var transform = element.Transform;

            if (transform)
            {
                transform.SetSiblingIndex(i);
            }
        }
    }

    private void OnSpawned(Poolee poolee)
    {
        _layoutPoolee = poolee;
        _layoutTransform = poolee.transform;

        SpawnElements();

        // Apply visibility
        _layoutPoolee.gameObject.SetActive(Visible);
    }
    
    private void SpawnElements()
    {
        foreach (var element in _elements)
        {
            element.Spawn(_layoutTransform);
        }

        // After all elements are spawned, resort them
        SortElements();
    }
    
    private void DespawnElements()
    {
        foreach (var element in _elements)
        {
            element.Despawn();
        }
    }

    public void UpdateScale(RigManager rigManager)
    {
        if (!Spawned)
        {
            return;
        }

        var avatar = rigManager.avatar;

        if (!avatar)
        {
            return;
        }

        float height = avatar.height / 1.76f;
        _layoutTransform.localScale = Vector3Extensions.one * height;
    }

    public void UpdateTransform(RigManager rigManager)
    {
        if (!Spawned)
        {
            return;
        }

        var head = rigManager.physicsRig.m_head;

        var position = head.position + Vector3Extensions.up * GetUIOffset(rigManager);

        var playerHead = RigData.Refs.Head;

        var rotation = Quaternion.LookRotation((position - playerHead.position).normalized, playerHead.up);

        _layoutTransform.SetPositionAndRotation(position, rotation);
    }

    public static float GetUIOffset(RigManager rigManager)
    {
        float offset = OffsetHeight;

        offset *= rigManager.avatar.height;

        return offset;
    }
}
