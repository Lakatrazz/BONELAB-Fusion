using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public static class NativeMessageTag
    {
        public static byte
            Unknown = 0,

            ConnectionRequest = 1,
            ConnectionResponse = 2,
            Disconnect = 3,

            PlayerRepTransform = 4,
            PlayerRepAvatar = 5,
            PlayerRepVitals = 6,

            PlayerRepGrab = 7,
            PlayerRepForceGrab = 8,
            PlayerRepRelease = 9,
            PlayerRepAnchors = 10,

            SceneLoad = 11,

            SyncableIDRequest = 12,
            SyncableIDResponse = 13,
            SyncableOwnershipRequest = 14,
            SyncableOwnershipResponse = 15,

            PropSyncableUpdate = 16,

            WorldGravity = 17,

            SpawnRequest = 18,
            SpawnResponse = 19,
            DespawnRequest = 20,
            DespawnResponse = 21,

            InventorySlotInsert = 22,
            InventorySlotDrop = 23,

            MagazineInsert = 24,
            MagazineEject = 25,

            GunShot = 26,

            ModuleMessage = 80;
    }

}
