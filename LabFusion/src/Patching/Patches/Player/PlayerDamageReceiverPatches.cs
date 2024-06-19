using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.Entities;

using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Player;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Combat;

using HarmonyLib;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PlayerDamageReceiver))]
public static class PlayerDamageReceiverPatches
{
    public static bool IsSynced(AttackType type)
    {
        return type == AttackType.Blunt || type == AttackType.Piercing;
    }

    [HarmonyPatch(nameof(PlayerDamageReceiver.ReceiveAttack))]
    [HarmonyPrefix]
    public static bool ReceiveAttack(PlayerDamageReceiver __instance, ref Attack attack)
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
                if (NetworkPlayerManager.TryGetPlayer(shooter, out var player) && !player.NetworkEntity.IsOwner)
                {
                    FusionPlayer.LastAttacker = player.PlayerId;

                    // Only allow manual damage for certain types
                    if (IsSynced(attack.attackType))
                    {
                        attack.damage = 0f;
                        return true;
                    }
                }
                // Were we hit by ourselves?
                else
                {
                    FusionPlayer.LastAttacker = null;
                }
            }
            // Is the attacked person another player? Did we attack them?
            else if (IsSynced(attack.attackType) && NetworkPlayerManager.TryGetPlayer(rm, out var player) && shooter.IsSelf())
            {
                // Send the damage over the network
                PlayerSender.SendPlayerDamage(player.PlayerId, attack, __instance.bodyPart);

                PlayerSender.SendPlayerAction(PlayerActionType.DEALT_DAMAGE_TO_OTHER_PLAYER, player.PlayerId);
            }
        }

        return true;
    }
}