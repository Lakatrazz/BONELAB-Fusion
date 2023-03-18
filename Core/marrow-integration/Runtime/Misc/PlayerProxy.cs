using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Data;
using LabFusion.Network;

using SLZ.Marrow.Pool;

using LabFusion.Utilities;
using LabFusion.Syncables;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Player Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class PlayerProxy : FusionMarrowBehaviour
    {
#if MELONLOADER
        public PlayerProxy(IntPtr intPtr) : base(intPtr) { }

        public void ClearInventory()
        {
            if (RigData.HasPlayer && NetworkInfo.HasServer) {
                var slots = RigData.RigReferences.RigSlots;

                for (var i = 0; i < slots.Length; i++) {
                    var slot = slots[i];

                    if (!slot._slottedWeapon)
                        continue;

                    var host = slot._weaponHost;
                    slot.DropWeapon();

                    var assetPoolee = AssetPoolee.Cache.Get(host.GetHostGameObject());

                    if (assetPoolee != null) {
                        var syncable = PropSyncable.Cache.Get(assetPoolee.gameObject);

                        if (syncable != null)
                            PooleeUtilities.SendDespawn(syncable.GetId());
                    }
                }
            }
        }

#else
        public override string Comment => "This proxy lets you manually control events of the local player in the scene.\n" +
            "For example, through a UnityEvent or UltEvent you could clear the player's inventory.\n" +
            "This may be useful for gamemodes that end and you need all of the items in their inventory to be gone.";

        public void ClearInventory() { }
#endif
    }
}
