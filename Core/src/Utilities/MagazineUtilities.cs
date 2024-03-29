using BoneLib.Nullables;
using LabFusion.Data;
using LabFusion.Extensions;
using SLZ;
using SLZ.Interaction;
using SLZ.Marrow.Data;
using SLZ.Props.Weapons;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    public static class MagazineUtilities
    {
        public static void ClaimMagazine(Magazine magazine, RigManager rigManager)
        {
            var ammoInventory = rigManager.AmmoInventory;

            CartridgeData cart = ammoInventory.ammoReceiver.defaultLightCart;

            if (ammoInventory.ammoReceiver._selectedCartridgeData != null)
                cart = ammoInventory.ammoReceiver._selectedCartridgeData;

            magazine.Initialize(cart, ammoInventory.GetCartridgeCount(cart));
            magazine.Claim();

            NullableMethodExtensions.AudioPlayer_PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, null, null, false, null, null);
        }

        public static void GrabMagazine(Magazine magazine, RigManager rigManager, Handedness handedness)
        {
            ClaimMagazine(magazine, rigManager);

            // Attach the object to the hand
            var grip = magazine.grip;

            var found = handedness == Handedness.LEFT ? rigManager.physicsRig.leftHand : rigManager.physicsRig.rightHand;

            if (found)
            {
                found.GrabLock = false;

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
