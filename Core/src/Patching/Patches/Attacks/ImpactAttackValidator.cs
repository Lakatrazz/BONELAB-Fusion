using LabFusion.MarrowIntegration;
using LabFusion.Utilities;
using SLZ.Combat;
using SLZ.Interaction;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Patching
{
    public static class ImpactAttackValidator
    {
        public static bool ValidateAttack(GameObject weaponObject, InteractableHost weaponHost, ImpactProperties hitObject)
        {
            var physRig = hitObject.GetComponentInParent<PhysicsRig>();

            // Was a player damaged? Make sure another player is holding the weapon
            if (physRig == null)
            {
                return true;
            }

            // Check if we can force enable
            if (AlwaysAllowImpactDamage.Cache.ContainsSource(weaponObject))
                return true;

            // Check if the hit player is grabbing this
            foreach (var hand in weaponHost._hands)
            {
                if (hand.manager != physRig.manager)
                    return true;
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
}
