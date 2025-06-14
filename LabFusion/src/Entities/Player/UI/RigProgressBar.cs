using System.Collections;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using Il2CppTMPro;

using LabFusion.Marrow;
using LabFusion.UI;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Entities;

public class RigProgressBar : IPopupLayoutElement, IProgress<float>
{
    private const string _visibilityParameterName = "Visible";

    private Poolee _poolee;
    private Transform _transform;

    private Slider _slider;
    private TMP_Text _text;

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

            if (value)
            {
                _poolee.gameObject.SetActive(true);
            }
            else
            {
                MelonCoroutines.Start(CoDelayedDisable(1.2f));
            }

            _animator.SetBool(_visibilityParameterName, Visible);
        }
    }

    private IEnumerator CoDelayedDisable(float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        if (_poolee != null)
        {
            _poolee.gameObject.SetActive(false);
        }
    }

    private float _progress = 0f;

    public void Spawn(Transform parent)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.ProgressBarReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            _poolee = poolee;
            _animator = poolee.GetComponent<Animator>();
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
        _animator = null;
        _transform = null;
        _slider = null;
    }

    public void Report(float value)
    {
        _progress = value;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_slider != null)
        {
            _slider.value = _progress;
        }

        if (_text != null)
        {
            _text.text = $"{Mathf.RoundToInt(_progress * 100f)}%";
        }

        if (_animator != null)
        {
            _animator.SetBool(_visibilityParameterName, Visible);
        }
    }
}