namespace LabFusion.Network;

public static class NativeMessageTag
{
    public static readonly byte
        // Built in messages
        Unknown = 0,
        
        // Connection messages
        // These should never change, they aren't game specific
        ConnectionRequest = 1,
        ConnectionResponse = 2,
        Disconnect = 3,

        PlayerPoseUpdate = 4,
        PlayerRepAvatar = 5,
        PlayerRepVitals = 6,
        PlayerRepRagdoll = 7,
        PlayerRepSeat = 8,

        PlayerRepGrab = 9,
        PlayerRepRelease = 10,
        PlayerRepAnchors = 11,

        SceneLoad = 12,

        EntityUnqueueRequest = 13,
        EntityUnqueueResponse = 14,
        EntityOwnershipRequest = 15,
        EntityOwnershipResponse = 16,

        EntityPoseUpdate = 17,
        NetworkPropCreate = 18,

        EntityZoneRegister = 19,

        SpawnRequest = 20,
        SpawnResponse = 21,
        DespawnRequest = 22,
        DespawnResponse = 23,

        InventorySlotInsert = 24,
        InventorySlotDrop = 25,
        InventoryAmmoReceiverDrop = 26,

        MagazineInsert = 27,
        MagazineEject = 28,

        GunShot = 29,

        PuppetMasterKill = 30,

        ObjectDestructibleDestroy = 32,

        ArenaTransition = 33,
        ChallengeSelect = 34,
        ArenaMenu = 35,
        GeoSelect = 36,

        DescentNoose = 37,
        DescentElevator = 38,

        MagmaGateEvent = 40,

        DescentIntro = 41,

        BodyLogEffect = 42,

        BonelabHubEvent = 43,

        PlayerSettings = 44,
        ServerSettings = 45,

        KartRaceEvent = 46,

        KeySlot = 48,

        NimbusGunNoclip = 49,

        PlayerRepAction = 50,

        ConstrainerMode = 51,
        ConstraintCreate = 52,
        ConstraintDelete = 53,

        BoardGenerator = 54,

        HomeEvent = 55,

        CrateSpawner = 56,

        TimeScale = 57,
        SlowMoButton = 58,

        PlayerMetadataRequest = 59,
        PlayerMetadataResponse = 60,

        TrialSpawnerEvents = 61,

        LevelRequest = 62,

        BodyLogToggle = 63,

        PlayerRepDamage = 64,

        TimeTrial_GameController = 65,
        BaseGameController = 66,

        PlayerVoiceChat = 67,

        PermissionCommandRequest = 68,

        PlayerRepTeleport = 69,

        HolodeckEvent = 71,

        SpawnGunSelect = 72,

        MineDiveCart = 73,

        MagazineClaim = 76,

        // Mod downloading
        ModInfoRequest = 77,
        ModInfoResponse = 78,

        // SDK messages
        // Module setup
        Module = 200,
        DynamicsAssignment = 201,

        // Gamemodes
        GamemodeMetadataSet = 202,
        GamemodeMetadataRemove = 203,
        GamemodeTriggerResponse = 204,

        // Point items
        PointItemEquipState = 206,
        PointItemTrigger = 207,
        PointItemTriggerValue = 208,

        // RPC
        RPCEvent = 209;
}