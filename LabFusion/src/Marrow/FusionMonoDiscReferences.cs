using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow
{
    public static class FusionMonoDiscReferences
    {
        public static readonly MonoDiscReference LavaGangVictoryReference = new("Lakatrazz.FusionContent.MonoDisc.LavaGangTriumphs");

        public static readonly MonoDiscReference LavaGangFailureReference = new("Lakatrazz.FusionContent.MonoDisc.LavaGangFails");

        public static readonly MonoDiscReference SabrelakeVictoryReference = new("Lakatrazz.FusionContent.MonoDisc.SabrelakeTriumphs");

        public static readonly MonoDiscReference SabrelakeFailureReference = new("Lakatrazz.FusionContent.MonoDisc.SabrelakeFails");

        public static readonly MonoDiscReference ErmReference = new("Lakatrazz.FusionContent.MonoDisc.Erm");

        public static readonly MonoDiscReference FistfightFusionReference = new("Lakatrazz.FusionContent.MonoDisc.FistfightFusion");

        public static readonly MonoDiscReference GeoGrpFellDownTheStairsReference = new("Lakatrazz.FusionContent.MonoDisc.GeoGrpFellDownTheStairs");

        public static readonly MonoDiscReference[] CombatSongReferences = new MonoDiscReference[]
        {
            new("Lakatrazz.FusionContent.MonoDisc.BuggyPhysics"),
            new("Lakatrazz.FusionContent.MonoDisc.SicklyBugInitiative"),
            new("Lakatrazz.FusionContent.MonoDisc.SyntheticCaverns"),
            new("Lakatrazz.FusionContent.MonoDisc.WackyWillysWonderland"),
            new("Lakatrazz.FusionContent.MonoDisc.SmigglesInDespair"),
            new("Lakatrazz.FusionContent.MonoDisc.AppenBeyuge"),
        };

    }
}
