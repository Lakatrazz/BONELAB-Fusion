using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Marrow;
using LabFusion.UI;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Entities;

public class RigHealthBar : IPopupLayoutElement
{
    private Poolee _poolee;
    private Transform _transform;

    private Slider _slider;
    private TMP_Text _text;

    public int Priority => 1000;

    public Transform Transform => _transform;

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

            if (_poolee == null)
            {
                return;
            }

            if (value)
            {
                _poolee.gameObject.SetActive(true);
            }
            else
            {
                _poolee.gameObject.SetActive(false);
            }
        }
    }

    private float _health = 100f;

    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;

            UpdateVisuals();
        }
    }

    private float _maxHealth = 100f;

    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;

            UpdateVisuals();
        }
    }

    public float HealthPercent => Health / MaxHealth;

    public void Spawn(Transform parent)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.HealthBarReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _poolee = poolee;
            _slider = poolee.GetComponentInChildren<Slider>();
            _transform = poolee.transform;

            _transform.parent = parent;
            _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            _text = poolee.GetComponentInChildren<TMP_Text>();
            _text.font = PersistentAssetCreator.Font;

            UpdateVisuals();
        });
    }

    public void Despawn()
    {
        if (_poolee == null)
        {
            return;
        }

        _poolee.Despawn();

        _poolee = null;
        _transform = null;
        _slider = null;
    }

    private void UpdateVisuals()
    {
        if (_slider != null)
        {
            _slider.value = HealthPercent;
        }

        if (_text != null)
        {
            _text.text = $"{Mathf.RoundToInt(Health)}/{Mathf.RoundToInt(MaxHealth)}";
        }
    }
}