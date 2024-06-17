using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using UnityEngine;

namespace LabFusion.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class DestroySensor : MonoBehaviour
{
    public DestroySensor(IntPtr intPtr) : base(intPtr) { }

    private Action _onDestroyed = null;
    private bool _isDestroyed = false;

    [HideFromIl2Cpp]
    public void Hook(Action hook)
    {
        if (_isDestroyed)
        {
            hook();
        }
        else
        {
            _onDestroyed += hook;
        }
    }

    public void OnDestroy()
    {
        _onDestroyed?.Invoke();
        _onDestroyed = null;

        _isDestroyed = true;
    }
}