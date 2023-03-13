using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public static class NativeMessageTag
    {
        public static readonly byte
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

            DescentNoose = 39,
            DescentElevator = 40,

            FunicularControllerEvent = 41,

            MagmaGateEvent = 42,

            DescentIntro = 43,

            BodyLogEffect = 44,

            BonelabHubEvent = 45,

            PlayerSettings = 46,
            ServerSettings = 47,

            KartRaceEvent = 48,

            FlashlightToggle = 49,

            TwoButtonRemoteControllerEvent = 50,

            KeySlot = 51,

            NimbusGunNoclip = 52,

            PlayerRepAction = 53,

            ConstrainerMode = 54,
            ConstraintCreate = 55,
            ConstraintDelete = 56,

            BoardCreate = 57,

            HomeEvent = 58,

            PowerableJointVoltage = 59,

            SpawnableCratePlacer = 60,

            TimeScale = 61,
            SlowMoButton = 62,

            BehaviourBaseNavLoco = 63,
            BehaviourBaseNavMental = 64,

            PlayerMetadataRequest = 65,
            PlayerMetadataResponse = 66,

            TrialSpawnerEvents = 67,

            LevelRequest = 68,

            PropSyncableSleep = 69,

            BodyLogToggle = 70,

            PlayerRepDamage = 71,

            TimeTrial_GameController = 72,
            BaseGameController = 73,

            PlayerVoiceChat = 74,

            PermissionCommandRequest = 75,

            PlayerRepTeleport = 76,

            ZoneEncounterEvent = 77,

            HolodeckEvent = 78,

            SpawnGunPreviewMesh = 79,

            // SDK messages
            // Module setup
            Module = 200,
            DynamicsAssignment = 201,

            // Gamemodes
            GamemodeMetadataSet = 202,
            GamemodeMetadataRemove = 203,
            GamemodeTriggerResponse = 204,

            // SDK scripts
            GameObjectActive = 205,

            // Point items
            PointItemEquipState = 206,
            PointItemTrigger = 207,
            PointItemTriggerValue = 208;
    }

}
