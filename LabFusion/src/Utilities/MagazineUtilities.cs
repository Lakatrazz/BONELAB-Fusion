using LabFusion.Extensions;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Marrow;
using Il2CppSLZ.Marrow.Audio;

namespace LabFusion.Utilities
{
    public static class MagazineUtilities
    {
        public static void ClaimMagazine(Magazine magazine, RigManager rigManager)
        {
            var ammoInventory = rigManager.GetComponentInChildren<AmmoInventory>(true);

            CartridgeData cart = ammoInventory.ammoReceiver.defaultLightCart;

            if (ammoInventory.ammoReceiver._selectedCartridgeData != null)
                cart = ammoInventory.ammoReceiver._selectedCartridgeData;

            magazine.Initialize(cart, ammoInventory.GetCartridgeCount(cart));
            magazine.Claim();

            SafeAudio3dPlayer.PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, Audio3dManager.softInteraction, 0.2f);
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
}
