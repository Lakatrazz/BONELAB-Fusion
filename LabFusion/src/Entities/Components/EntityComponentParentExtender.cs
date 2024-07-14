using UnityEngine;

namespace LabFusion.Entities;

public abstract class EntityComponentParentExtender<TComponent> : EntityComponentExtender<TComponent> where TComponent : Component
{
    protected override TComponent GetComponent(GameObject go)
    {
        return go.GetComponentInParent<TComponent>(true);
    }
}