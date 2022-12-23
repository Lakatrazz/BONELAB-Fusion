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
            PlayerRepGameworld = 5,
            PlayerRepAvatar = 6,
            PlayerRepVitals = 7,
            PlayerRepRagdoll = 8,
            PlayerRepSeat = 9,

            PlayerRepGrab = 10,
            PlayerRepForceGrab = 11,
            PlayerRepRelease = 12,
            PlayerRepAnchors = 13,

            SceneLoad = 14,

            SyncableIDRequest = 15,
            SyncableIDResponse = 16,
            SyncableOwnershipRequest = 17,
            SyncableOwnershipResponse = 18,

            PropSyncableUpdate = 19,
            PropSyncableCreate = 20,

            WorldGravity = 21,

            SpawnRequest = 22,
            SpawnResponse = 23,
            DespawnRequest = 24,
            DespawnResponse = 25,

            InventorySlotInsert = 26,
            InventorySlotDrop = 27,

            MagazineInsert = 28,
            MagazineEject = 29,

            GunShot = 30,

            PuppetMasterKill = 31,

            SimpleGripEvent = 32,

            PropHealthDestroy = 33,
            ObjectDestructableDestroy = 34,

            ArenaTransition = 35,
            ChallengeSelect = 36,
            ArenaMenu = 37,
            GeoSelect = 38,

            ModuleMessage = 80;
    }

}
