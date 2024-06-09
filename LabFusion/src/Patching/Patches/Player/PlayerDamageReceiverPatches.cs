using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.Representation;

using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Player;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Combat;

using HarmonyLib;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PlayerDamageReceiver))]
    public static class PlayerDamageReceiverPatches
    {
        [HarmonyPatch(nameof(PlayerDamageReceiver.ReceiveAttack))]
        [HarmonyPrefix]
        public static bool ReceiveAttack(PlayerDamageReceiver __instance, Attack attack)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            } 

            var rm = __instance.health._rigManager;

            // Get the attack and its shooter
            TriggerRefProxy proxy = attack.proxy;

            RigManager shooter = null;

            if (proxy != null && proxy.root != null)
            {
                shooter = RigManager.Cache.Get(proxy.root);
            }

            // Make sure we have the attacker and attacked
            if (rm != null && shooter != null)
            {
                // Is the attacked person us?
                if (rm.IsSelf())
                {
                    // Were we hit by another player?
                    if (PlayerRepManager.TryGetPlayerRep(shooter, out var rep))
                    {
                        FusionPlayer.LastAttacker = rep.PlayerId;

                        // Only allow manual bullet damage
                        if (attack.attackType == AttackType.Piercing)
                        {
                            return false;
                        }
                    }
                    // Were we hit by ourselves?
                    else
                    {
                        FusionPlayer.LastAttacker = null;
                    }
                }
                // Is the attacked person another player? Did we attack them?
                else if (PlayerRepManager.TryGetPlayerRep(rm, out var rep) && shooter.IsSelf())
                {
                    // Send the damage over the network
                    PlayerSender.SendPlayerDamage(rep.PlayerId, attack.damage, __instance.bodyPart);

                    PlayerSender.SendPlayerAction(PlayerActionType.DEALT_DAMAGE_TO_OTHER_PLAYER, rep.PlayerId);
                }
            }

            return true;
        }
    }
}
