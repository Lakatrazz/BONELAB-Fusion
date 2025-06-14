using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Player;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Combat;

using HarmonyLib;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PlayerDamageReceiver))]
public static class PlayerDamageReceiverPatches
{
    [HarmonyPatch(nameof(PlayerDamageReceiver.ReceiveAttack))]
    [HarmonyPrefix]
    public static bool ReceiveAttack(PlayerDamageReceiver __instance, ref Attack attack)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        } 

        var rm = __instance.health._rigManager;

        // Get the attack and its attacker
        TriggerRefProxy proxy = attack.proxy;

        RigManager attacker = null;

        if (proxy != null && proxy.root != null)
        {
            attacker = RigManager.Cache.Get(proxy.root);
        }

        // Make sure we have the attacker and attacked
        if (rm != null && attacker != null)
        {
            // Is the attacked person us?
            if (rm.IsLocalPlayer())
            {
                // Were we hit by another player?
                if (NetworkPlayerManager.TryGetPlayer(attacker, out var player) && !player.NetworkEntity.IsOwner)
                {
                    FusionPlayer.LastAttacker = player.PlayerID;

                    // Only allow manual damage
                    attack.damage = 0f;
                    return true;
                }
                // Were we hit by ourselves?
                else
                {
                    FusionPlayer.LastAttacker = null;
                }
            }
            // Is the attacked person another player? Did we attack them?
            else if (NetworkPlayerManager.TryGetPlayer(rm, out var player) && attacker.IsLocalPlayer())
            {
                // Don't attack the other player if friendly fire is disabled
                if (!NetworkCombatManager.CanAttack(player))
                {
                    return false;
                }

                // Send the damage over the network
                PlayerSender.SendPlayerDamage(player.PlayerID, attack, __instance.bodyPart);

                PlayerSender.SendPlayerAction(PlayerActionType.DEALT_DAMAGE_TO_OTHER_PLAYER, player.PlayerID);
            }
        }

        return true;
    }
}