using LabFusion.Network;

namespace LabFusion.Senders;

public static class ArenaSender
{
    public static void SendArenaTransition(ArenaTransitionType type)
    {
        if (NetworkInfo.IsHost)
        {
            var data = ArenaTransitionData.Create(type);

            MessageRelay.RelayNative(data, NativeMessageTag.ArenaTransition, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

    public static void SendChallengeSelect(byte menuIndex, byte challengeNumber, ChallengeSelectType type)
    {
        if (NetworkInfo.IsHost)
        {
            var data = ChallengeSelectData.Create(menuIndex, challengeNumber, type);

            MessageRelay.RelayNative(data, NativeMessageTag.ChallengeSelect, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

    public static void SendGeometryChange(byte geoIndex)
    {
        if (NetworkInfo.IsHost)
        {
            var data = GeoSelectData.Create(geoIndex);

            MessageRelay.RelayNative(data, NativeMessageTag.GeoSelect, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

    public static void SendMenuSelection(byte selectionNumber, ArenaMenuType type)
    {
        if (NetworkInfo.IsHost)
        {
            var data = ArenaMenuData.Create(selectionNumber, type);

            MessageRelay.RelayNative(data, NativeMessageTag.ArenaMenu, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }

}