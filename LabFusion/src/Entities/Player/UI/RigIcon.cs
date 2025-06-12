using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using LabFusion.UI;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Entities;

public class RigIcon : IPopupLayoutElement
{
    private Transform _iconTransform;
    private Poolee _iconPoolee;
    private RawImage _iconImage;

    public RawImage Image => _iconImage;

    private Texture _texture = null;
    public Texture Texture
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;

            UpdateImage();
        }
    }

    public int Priority => -1000;

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

            if (_iconPoolee == null)
            {
                return;
            }

            _iconPoolee.gameObject.SetActive(value);
        }
    }

    public Transform Transform => _iconTransform;

    public void UpdateImage()
    {
        if (Image == null)
        {
            return;
        }

        Image.texture = Texture;
    }

    public void Spawn(Transform parent)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.IconReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _iconPoolee = poolee;
            _iconImage = poolee.GetComponentInChildren<RawImage>();
            _iconTransform = poolee.transform;

            _iconTransform.parent = parent;
            _iconTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            UpdateImage();
        });
    }

    public void Despawn()
    {
        if (_iconPoolee == null)
        {
            return;
        }

        _iconPoolee.Despawn();

        _iconPoolee = null;
        _iconImage = null;
        _iconTransform = null;
    }
}
