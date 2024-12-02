using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes;

public class Juggernaut : Gamemode
{
    public override string Title => "Juggernaut";

    public override string Author => FusionMod.ModAuthor;

    public static class Defaults
    {
        public const float NormalVitality = 1f;

        public const float JuggernautVitality = 20f;
    }
}
