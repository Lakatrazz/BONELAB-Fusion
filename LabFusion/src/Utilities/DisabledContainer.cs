using UnityEngine;

namespace LabFusion.Utilities;

public static class DisabledContainer
{
    public static GameObject ContainerGameObject
    {
        get
        {
            if (_containerGameObject == null)
            {
                CreateContainer();
            }

            return _containerGameObject;
        }
    }

    public static Transform ContainerTransform
    {
        get
        {
            if (_containerTransform == null)
            {
                CreateContainer();
            }

            return _containerTransform;
        }
    }

    private static GameObject _containerGameObject = null;
    private static Transform _containerTransform = null;

    private static void CreateContainer()
    {
        _containerGameObject = new("Disabled Container");
        _containerGameObject.SetActive(false);

        _containerTransform = _containerGameObject.transform;
    }
}
