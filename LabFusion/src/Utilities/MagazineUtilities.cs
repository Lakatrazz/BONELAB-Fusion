using LabFusion.Extensions;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow;

using LabFusion.Marrow;

namespace LabFusion.Utilities;

public static class MagazineUtilities
{
    public static void ClaimMagazine(Magazine magazine, RigManager rigManager)
    {
        var ammoReceiver = rigManager.GetComponentInChildren<InventoryAmmoReceiver>(true);

        CartridgeData cart = ammoReceiver.defaultLightCart;

        if (ammoReceiver._selectedCartridgeData != null)
        {
            cart = ammoReceiver._selectedCartridgeData;
        }

        magazine.Initialize(cart, AmmoInventory.Instance.GetCartridgeCount(cart));
        magazine.Claim();

        SafeAudio3dPlayer.PlayAtPoint(ammoReceiver.grabClips, ammoReceiver.transform.position, Audio3dManager.softInteraction, 0.2f);
    }

    public static void GrabMagazine(Magazine magazine, RigManager rigManager, Handedness handedness)
    {
        ClaimMagazine(magazine, rigManager);

        // Attach the object to the hand
        var grip = magazine.grip;

        var found = handedness == Handedness.LEFT ? rigManager.physicsRig.leftHand : rigManager.physicsRig.rightHand;

        if (found)
        {
            // Delay by one frame to fix weird grabbing
            DelayUtilities.Delay(() =>
            {
                grip.MoveIntoHand(found);

                grip.TryAttach(found, true);
            }, 1);
        }
    }
}