using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class RigNameTag : IHeadUIElement
{
    private Transform _nametagTransform;
    private Poolee _nametagPoolee;
    private TextMeshProUGUI _nametagText;

    public TextMeshProUGUI Text => _nametagText;

    private Color _color = Color.white;
    public Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;

            if (Text == null)
            {
                return;
            }

            Text.color = value;
        }
    }

    private string _username = "No Name";

    public int Priority => 0;

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

            if (_nametagPoolee == null)
            {
                return;
            }

            _nametagPoolee.gameObject.SetActive(value);
        }
    }

    public Transform Transform => _nametagTransform;

    public void SetUsername(string username)
    {
        _username = username;

        UpdateText();
    }

    public void UpdateText()
    {
        if (Text == null)
        {
            return;
        }

        // Only allow color
        Text.text = _username.RemoveRichTextExceptColor();

        // Update multiply color value
        Text.color = Color;
    }

    public void Spawn(Transform parent)
    {
        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.NameTagReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _nametagPoolee = poolee;
            _nametagText = poolee.GetComponentInChildren<TextMeshProUGUI>();
            _nametagTransform = poolee.transform;

            _nametagTransform.parent = parent;
            _nametagTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            _nametagText.font = PersistentAssetCreator.Font;

            UpdateText();
        });
    }

    public void Despawn()
    {
        if (_nametagPoolee == null)
        {
            return;
        }

        _nametagPoolee.Despawn();

        _nametagPoolee = null;
        _nametagText = null;
        _nametagTransform = null;
    }
}
