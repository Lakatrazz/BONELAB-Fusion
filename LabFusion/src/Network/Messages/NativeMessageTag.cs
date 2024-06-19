namespace LabFusion.Network;

public static class NativeMessageTag
{
    public static readonly byte
        // Built in messages
        Unknown = 0,

        ConnectionRequest = 1,
        ConnectionResponse = 2,
        Disconnect = 3,

        PlayerPoseUpdate = 4,
        PlayerRepAvatar = 6,
        PlayerRepVitals = 7,
        PlayerRepRagdoll = 8,
        PlayerRepSeat = 9,

        PlayerRepGrab = 10,
        PlayerRepRelease = 12,
        PlayerRepAnchors = 13,

        SceneLoad = 14,

        EntityUnqueueRequest = 15,
        EntityUnqueueResponse = 16,
        EntityOwnershipRequest = 17,
        EntityOwnershipResponse = 18,

        EntityPoseUpdate = 19,
        PropSyncableCreate = 20,

        EntityZoneRegister = 21,

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

        KeySlot = 51,

        NimbusGunNoclip = 52,

        PlayerRepAction = 53,

        ConstrainerMode = 54,
        ConstraintCreate = 55,
        ConstraintDelete = 56,

        HomeEvent = 58,

        CrateSpawner = 60,

        TimeScale = 61,
        SlowMoButton = 62,

        PlayerMetadataRequest = 65,
        PlayerMetadataResponse = 66,

        TrialSpawnerEvents = 67,

        LevelRequest = 68,

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

        MineDiveCart = 80,

        // Vote kicking
        VoteKickRequest = 81,
        VoteKickResponse = 82,

        MagazineClaim = 83,

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
        PointItemTriggerValue = 208;
}