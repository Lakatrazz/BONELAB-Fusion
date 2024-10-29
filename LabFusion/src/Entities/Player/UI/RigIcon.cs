using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Entities;

public class RigIcon : IHeadUIElement
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

        Image.texture = null; // Replace with set texture
    }

    public void Spawn(Transform parent)
    {
        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.IconReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
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
