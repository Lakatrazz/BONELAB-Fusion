using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Marrow.Patching;

public static class ImpactAttackValidator
{
    public static bool ValidateImpact(GameObject weaponObject, InteractableHost weaponHost, ImpactProperties hitObject)
    {
        var physRig = hitObject.GetComponentInParent<PhysicsRig>();

        // Was a player damaged? Make sure another player is holding the weapon
        if (physRig == null)
        {
            return true;
        }

        // Check if the hit player is grabbing this
        foreach (var hand in weaponHost._hands)
        {
            if (hand.manager != physRig.manager)
            {
                return true;
            }
        }

        // Check the last grabbed proxy
        var proxy = weaponHost.LastGrabbedProxy;
        if (proxy != null)
        {
            bool isLastHolder = RigManager.Cache.Get(proxy.root) == physRig.manager;

            return !isLastHolder;
        }

        return false;
    }
}
