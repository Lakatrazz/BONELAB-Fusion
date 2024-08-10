using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Marrow;
using LabFusion.Utilities;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Entities;

public class RigProgressBar : IHeadUIElement, IProgress<float>
{
    private const string _animatorParameterName = "Visible";

    private Poolee _poolee;
    private Transform _transform;

    private Slider _slider;

    private Animator _animator;

    public int Priority => 10000;

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

            _poolee.gameObject.SetActive(value);
        }
    }

    private float _progress = 0f;

    public void Spawn(Transform parent)
    {
        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.ProgressBarReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _poolee = poolee;
            _animator = poolee.GetComponent<Animator>();
            _slider = poolee.GetComponentInChildren<Slider>();
            _transform = poolee.transform;

            _transform.parent = parent;
            _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            poolee.gameObject.SetActive(Visible);

            var text = poolee.GetComponentInChildren<TMP_Text>();
            text.font = PersistentAssetCreator.Font;

            _slider.value = _progress;

            _animator.SetBool(_animatorParameterName, true);
        });
    }

    public void Despawn()
    {
        if (_poolee == null)
        {
            return;
        }

        _animator.SetBool(_animatorParameterName, false);

        _poolee = null;
        _animator = null;
        _transform = null;
        _slider = null;

        PooleeHelper.DespawnDelayed(_poolee, 1.5f);
    }

    public void Report(float value)
    {
        _progress = value;

        if (_slider == null)
        {
            return;
        }

        _slider.value = value;
    }
}