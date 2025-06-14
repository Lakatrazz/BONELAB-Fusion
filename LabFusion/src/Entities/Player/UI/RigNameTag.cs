using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.UI;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Entities;

public class RigNameTag : IPopupLayoutElement
{
    private Transform _nametagTransform = null;
    private Poolee _nametagPoolee = null;
    private TextMeshProUGUI _nametagText = null;

    private GameObject _crownGameObject = null;

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
    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            _username = value;

            UpdateText();
        }
    }

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

    private bool _crownVisible = false;
    public bool CrownVisible
    {
        get
        {
            return _crownVisible;
        }
        set
        {
            _crownVisible = value;

            if (_crownGameObject == null)
            {
                return;
            }

            _crownGameObject.SetActive(value);
        }
    }

    public Transform Transform => _nametagTransform;

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
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.NameTagReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _nametagPoolee = poolee;
            _nametagText = poolee.GetComponentInChildren<TextMeshProUGUI>();
            _nametagTransform = poolee.transform;

            _nametagTransform.parent = parent;
            _nametagTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            _nametagText.font = PersistentAssetCreator.Font;

            GetIcons(_nametagText.transform);

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

    private void GetIcons(Transform textTransform)
    {
        var icons = textTransform.Find("Icons");

        if (icons == null)
        {
            return;
        }

        var crownTransform = icons.Find("Crown");

        if (crownTransform != null)
        {
            _crownGameObject = crownTransform.gameObject;

            _crownGameObject.SetActive(CrownVisible);
        }
    }
}
