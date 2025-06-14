using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Marrow;
using LabFusion.UI;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Entities;

public class RigLivesBar : IPopupLayoutElement
{
    private Poolee _poolee;
    private Transform _transform;

    private GameObject _lifeTemplate = null;
    private TMP_Text _damageText = null;

    private readonly List<GameObject> _lifeInstances = new();

    public int Priority => 500;

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

    private float _damage = 0f;
    public float Damage
    {
        get
        {
            return _damage;
        }
        set
        {
            _damage = value;

            UpdateVisuals();
        }
    }

    private int _lives = 0;
    public int Lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;

            UpdateVisuals();
        }
    }

    public void Spawn(Transform parent)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.LivesBarReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            ClearLifeInstances();

            _poolee = poolee;
            _transform = poolee.transform;

            _transform.parent = parent;
            _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            _lifeTemplate = _transform.Find("Canvas/Icons/Life").gameObject;
            _lifeTemplate.SetActive(false);

            _damageText = poolee.GetComponentInChildren<TMP_Text>();
            _damageText.font = PersistentAssetCreator.Font;

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

        ClearLifeInstances();
    }

    private void UpdateVisuals()
    {
        CreateLifeInstances(Lives);

        if (_damageText != null)
        {
            _damageText.text = $"{Mathf.RoundToInt(Damage)}%";
            _damageText.color = Color.Lerp(Color.white, new Color(0.6f, 0f, 0f, 1f), Damage / 300f);
        }
    }

    private void ClearLifeInstances()
    {
        foreach (var life in _lifeInstances)
        {
            if (life != null)
            {
                GameObject.Destroy(life);
            }
        }

        _lifeInstances.Clear();
    }

    private void CreateLifeInstances(int count)
    {
        if (count == _lifeInstances.Count)
        {
            return;
        }

        if (_lifeTemplate == null)
        {
            return;
        }

        ClearLifeInstances();

        for (var i = 0; i < count; i++)
        {
            var life = GameObject.Instantiate(_lifeTemplate, _lifeTemplate.transform.parent, false);
            life.transform.SetSiblingIndex(i);

            life.SetActive(true);

            _lifeInstances.Add(life);
        }
    }
}