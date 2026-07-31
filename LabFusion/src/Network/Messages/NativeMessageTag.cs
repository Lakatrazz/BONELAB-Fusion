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

        PlayerVoiceChat = 67,

        PermissionCommandRequest = 68,

        ModInfoRequest = 77,
        ModInfoResponse = 78,

        EntityDataRequest = 79,
        EntityCullStatus = 80,

        // SDK messages
        // Module setup
        Module = 200,
        DynamicsAssignment = 201,

        // Gamemodes
        GamemodeMetadataSet = 202,
        GamemodeMetadataRemove = 203,
        GamemodeTriggerResponse = 204,

        // RPC
        RPCEvent = 209,
        RPCInt = 210,
        RPCFloat = 211,
        RPCBool = 212,
        RPCString = 213,
        RPCVector3 = 214,
        RPCMethod = 215;
}