using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes;

public class HideAndSeek : Gamemode
{
    public override string GamemodeName => "Hide And Seek";

    public override string GamemodeCategory => "Fusion";

    public static class Defaults
    {
        public const int SeekerCount = 2;
    }

    public int SeekerCount { get; set; } = Defaults.SeekerCount;
}
