using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Entities;

public class RigArt
{
    private RigManager _rigManager = null;
    private Renderer[] _physicsRenderers = null;
    private InventoryAmmoReceiver _ammoReceiver = null;

    public RigArt(RigManager rigManager)
    {
        _rigManager = rigManager;

        // Collect ammo receiver
        _ammoReceiver = rigManager.physicsRig.GetComponentInChildren<InventoryAmmoReceiver>(true);

        // Collect physics renderers
        List<Renderer> renderers = new();

        foreach (var renderer in _rigManager.physicsRig.GetComponentsInChildren<Renderer>())
        {
            if (renderer.enabled)
            {
                renderers.Add(renderer);
            }
        }

        _physicsRenderers = renderers.ToArray();
    }

    public void CullArt(bool isInactive)
    {
        bool enabled = !isInactive;

        TogglePhysicsRenderers(enabled);
        ToggleAvatar(enabled);
        ToggleAmmoPouch(enabled);
    }

    private void TogglePhysicsRenderers(bool enabled)
    {
        if (_physicsRenderers == null)
        {
            return;
        }

        foreach (var renderer in _physicsRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = enabled;
        }
    }

    private void ToggleAvatar(bool enabled)
    {
        if (_rigManager == null || _rigManager.avatar == null)
        {
            return;
        }

        _rigManager.avatar.gameObject.SetActive(enabled);
    }

    private void ToggleAmmoPouch(bool enabled)
    {
        if (_ammoReceiver == null)
        {
            return;
        }

        _ammoReceiver.gameObject.SetActive(enabled);
    }
}