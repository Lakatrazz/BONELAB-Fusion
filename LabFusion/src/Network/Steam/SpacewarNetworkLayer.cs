namespace LabFusion.Network
{
    public class SpacewarNetworkLayer : SteamNetworkLayer
    {
        public const int SpacewarId = 480;

        public override uint ApplicationID => SpacewarId;

        internal override string Title => "Spacewar";
    }
}
