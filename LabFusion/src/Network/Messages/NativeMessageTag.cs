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
        PhysicsRigState = 7,
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

        PlayerSettings = 44,
        ServerSettings = 45,

        PlayerRepAction = 50,

        SlowMoButton = 58,

        PlayerMetadataRequest = 59,
        PlayerMetadataResponse = 60,

        LevelRequest = 62,

        PlayerRepDamage = 64,

        PlayerVoiceChat = 67,

        PermissionCommandRequest = 68,

        PlayerRepTeleport = 69,

        ModInfoRequest = 77,
        ModInfoResponse = 78,

        EntityDataRequest = 79,

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
        RPCEvent = 209,
        RPCInt = 210,
        RPCFloat = 211,
        RPCBool = 212,
        RPCString = 213,
        RPCVector3 = 214,
        RPCMethod = 215;
}