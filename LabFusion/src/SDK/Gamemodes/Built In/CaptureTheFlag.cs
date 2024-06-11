using LabFusion.SDK.Gamemodes;
using UnityEngine;

namespace LabFusion.Core.Gamemodes
{
    public class CaptureTheFlag : Gamemode
    {
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Capture The Flag";

        public Team RedTeam => new Team("Red", Color.red, 6);
        public Team BlueTeam => new Team("Blue", Color.blue, 6);

        public override bool VisibleInBonemenu => false;
    }
}
