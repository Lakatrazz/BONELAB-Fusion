using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Marrow;
using LabFusion.Extensions;
using LabFusion.Entities;

namespace LabFusion.Utilities;

public static class MagazineUtilities
{
    public static void ClaimMagazine(Magazine magazine, InventoryAmmoReceiver ammoReceiver)
    {
        CartridgeData cart = ammoReceiver.defaultLightCart;

        if (ammoReceiver._selectedCartridgeData != null)
        {
            cart = ammoReceiver._selectedCartridgeData;
        }

        if (cart != null && cart.projectile != null)
        {
            ProjectileEmitter.Register(cart.projectile);
        }

        magazine.Initialize(cart, AmmoInventory.Instance.GetCartridgeCount(cart));
        magazine.Claim();

        LocalAudioPlayer.PlayAtPoint(ammoReceiver.grabClips, ammoReceiver.transform.position, new AudioPlayerSettings()
        {
            Mixer = LocalAudioPlayer.SoftInteraction,
            Volume = 0.2f,
        });
    }

    public static void GrabMagazine(Magazine magazine, NetworkPlayer player, Handedness handedness)
    {
        player.HookOnReady(OnPlayerReady);

        void OnPlayerReady()
        {
            var rigManager = player.RigRefs.RigManager;

            var ammoReceiverExtender = player.NetworkEntity.GetExtender<InventoryAmmoReceiverExtender>();

            if (ammoReceiverExtender != null)
            {
                ClaimMagazine(magazine, ammoReceiverExtender.Component);
            }

            // Attach the object to the hand
            var grip = magazine.grip;

            var found = handedness == Handedness.UNDEFINED ? null : handedness == Handedness.LEFT ? rigManager.physicsRig.leftHand : rigManager.physicsRig.rightHand;

            if (found)
            {
                // Delay by one frame to fix weird grabbing
                DelayUtilities.InvokeNextFrame(() =>
                {
                    grip.MoveIntoHand(found);

                    grip.TryAttach(found, true);
                });
            }
        }
    }
}