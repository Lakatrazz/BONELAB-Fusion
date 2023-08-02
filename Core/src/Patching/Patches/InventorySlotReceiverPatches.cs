﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.Syncables;
using LabFusion.Utilities;

using Newtonsoft.Json.Linq;
using SLZ;
using SLZ.Bonelab;
using SLZ.Interaction;
using SLZ.Player;
using SLZ.Props.Weapons;
using SLZ.Rig;
using SLZ.VRMK;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(InventorySlotReceiver))]
    public class InventorySlotReceiverPatches
    {
        public static bool IgnorePatches = false;

        private static void OnDropWeapon(InventorySlotReceiver __instance, Hand hand = null) {
            if (IgnorePatches)
                return;

            try
            {
                if (NetworkInfo.HasServer && __instance._slottedWeapon)
                {
                    var rigManager = __instance.GetComponentInParent<RigManager>();

                    if (rigManager != null)
                    {
                        bool isAvatarSlot = __instance.GetComponentInParent<Avatar>() != null;

                        byte? smallId = null;
                        RigReferenceCollection references = null;

                        if (rigManager == RigData.RigReferences.RigManager)
                        {
                            smallId = PlayerIdManager.LocalSmallId;
                            references = RigData.RigReferences;
                        }
                        else if (PlayerRepManager.TryGetPlayerRep(rigManager, out var rep))
                        {
                            smallId = rep.PlayerId.SmallId;
                            references = rep.RigReferences;
                        }

                        if (!smallId.HasValue)
                            return;

                        byte? index = references.GetIndex(__instance, isAvatarSlot);

                        if (!index.HasValue)
                            return;

                        var handedness = Handedness.UNDEFINED;
                        if (hand != null) {
                            handedness = hand.handedness;

                            // Reward achievement
                            if (!rigManager.IsSelf() && AchievementManager.TryGetAchievement<HighwayMan>(out var achievement)) {
                                achievement.IncrementTask();
                            }
                        }

                        using var writer = FusionWriter.Create(InventorySlotDropData.Size);
                        using var data = InventorySlotDropData.Create(smallId.Value, PlayerIdManager.LocalSmallId, index.Value, handedness, isAvatarSlot);
                        writer.Write(data);

                        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotDrop, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("patching InventorySlotReceiver.OnHandGrab", e);
#endif
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventorySlotReceiver.DropWeapon))]
        public static void DropWeaponPrefix(InventorySlotReceiver __instance) {
            OnDropWeapon(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventorySlotReceiver.OnHandGrab))]
        public static void OnHandGrabPrefix(InventorySlotReceiver __instance, Hand hand) {
            OnDropWeapon(__instance, hand);
        }
    }

    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandDrop))]
    public class InventorySlotReceiverDrop
    {
        public static bool PreventInsertCheck = false;

        public static void Postfix(InventorySlotReceiver __instance, IGrippable host)
        {
            if (PreventInsertCheck)
                return;

            try
            {
                if (NetworkInfo.HasServer && __instance._slottedWeapon && WeaponSlotExtender.Cache.TryGet(__instance._slottedWeapon, out var syncable)) {
                    var rigManager = __instance.GetComponentInParent<RigManager>();

                    if (rigManager != null) {
                        bool isAvatarSlot = __instance.GetComponentInParent<Avatar>() != null;

                        byte? smallId = null;
                        RigReferenceCollection references = null;

                        if (rigManager == RigData.RigReferences.RigManager) {
                            smallId = PlayerIdManager.LocalSmallId;
                            references = RigData.RigReferences;
                        }
                        else if (PlayerRepManager.TryGetPlayerRep(rigManager, out var rep)) {
                            smallId = rep.PlayerId.SmallId;
                            references = rep.RigReferences;
                        }

                        if (!smallId.HasValue)
                            return; 

                        byte? index = references.GetIndex(__instance, isAvatarSlot);

                        if (!index.HasValue)
                            return;


                        using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
                        using var data = InventorySlotInsertData.Create(smallId.Value, PlayerIdManager.LocalSmallId, syncable.Id, index.Value, isAvatarSlot);
                        writer.Write(data);

                        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("patching InventorySlotReceiver.OnHandDrop", e);
#endif
            }
        }
    }
}
