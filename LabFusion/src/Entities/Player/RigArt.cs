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
        if (isInactive)
        {
            // Hide the renderers on the physics rig like holsters
            foreach (var renderer in _physicsRenderers)
            {
                renderer.enabled = false;
            }

            // Hide the avatar
            _rigManager.avatar.gameObject.SetActive(false);

            // Hide the ammo pouch
            _ammoReceiver.gameObject.SetActive(false);
        }
        else
        {
            // Show the holster renderers
            foreach (var renderer in _physicsRenderers)
            {
                renderer.enabled = true;
            }

            // Show the avatar
            _rigManager.avatar.gameObject.SetActive(true);

            // Show the ammo pouch
            _ammoReceiver.gameObject.SetActive(true);
        }
    }
}