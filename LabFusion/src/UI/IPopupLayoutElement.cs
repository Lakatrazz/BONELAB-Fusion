using UnityEngine;

namespace LabFusion.UI;

public interface IPopupLayoutElement
{
    int Priority { get; }

    Transform Transform { get; }

    bool Visible { get; set; }

    void Spawn(Transform parent);

    void Despawn();
}