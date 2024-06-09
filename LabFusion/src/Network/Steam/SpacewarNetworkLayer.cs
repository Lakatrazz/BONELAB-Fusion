namespace LabFusion.Network
{
    public class SpacewarNetworkLayer : SteamNetworkLayer
    {
        public const int SpacewarId = 480;

        public override uint ApplicationID => SpacewarId;

        public override string Title => "Spacewar";
    }
}
