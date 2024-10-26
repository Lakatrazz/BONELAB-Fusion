using HarmonyLib;

using Il2CppSLZ.Marrow.Circuits;

using LabFusion.Bonelab;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(ButtonController))]
public static class ButtonControllerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ButtonController.ReadSensor))]
    public static void ReadSensorPrefix(ButtonController __instance, double fixedTime, float lastSensorValue)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var networkEntity = ButtonControllerExtender.Cache.Get(__instance);

        if (networkEntity == null)
        {
            return;
        }

        var extender = networkEntity.GetExtender<ButtonControllerExtender>();

        if (networkEntity.IsOwner)
        {
            OnOwnedButtonUpdate(__instance, extender);
        }
        else
        {
            OnReceivedButtonUpdate(__instance, extender);
        }
    }

    private static void OnOwnedButtonUpdate(ButtonController controller, ButtonControllerExtender extender)
    {
        // State change detected
        if (extender.Charged != controller._charged)
        {
            // Send button message
            using var writer = FusionWriter.Create(ButtonChargeData.Size);
            var data = ButtonChargeData.Create(PlayerIdManager.LocalSmallId, extender.NetworkEntity.Id, controller._charged);
            writer.Write(data);

            using var message = FusionMessage.ModuleCreate<ButtonChargeMessage>(writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);

            // Update the extender state
            extender.Charged = controller._charged;
        }
    }

    private static void OnReceivedButtonUpdate(ButtonController controller, ButtonControllerExtender extender)
    {
        bool charged = extender.Charged;

        var limit = controller._servo.joint.linearLimit.limit;

        var joint = controller._servo.joint;

        // TODO: probably add sleep checks later for better performance
        switch (controller.buttonMode)
        {
            case ButtonController.ButtonMode.ClickDown:
            case ButtonController.ButtonMode.ClickUp:
                joint.targetPosition = Vector3.right * ProcessClickButton(limit, charged);
                break;
            case ButtonController.ButtonMode.Toggle:
                joint.targetPosition = Vector3.right * ProcessToggleButton(controller, limit, charged);
                break;
        }
    }

    private static float ProcessClickButton(float limit, bool charged)
    {
        if (charged)
        {
            return -limit;
        }

        return limit;
    }

    private static float ProcessToggleButton(ButtonController controller, float limit, bool charged)
    {
        if (controller._charged != charged && controller._toggleCharged)
        {
            return -limit;
        }

        return limit;
    }
}
