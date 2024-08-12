using System.Reflection;
using System.Runtime.InteropServices;

using MelonLoader;
using LabFusion;

[assembly: Guid("490e160d-251d-4ab4-a3bb-f473961ff8a1")]
[assembly: AssemblyTitle(FusionMod.ModName)]
[assembly: AssemblyVersion(FusionVersion.VersionString)]
[assembly: AssemblyFileVersion(FusionVersion.VersionString)]

[assembly: MelonInfo(typeof(FusionMod), FusionMod.ModName, FusionVersion.VersionString, FusionMod.ModAuthor)]
[assembly: MelonGame(FusionMod.GameDeveloper, FusionMod.GameName)]
[assembly: MelonPriority(-10000)]
[assembly: MelonOptionalDependencies("System.Windows.Forms")]
[assembly: MelonIncompatibleAssemblies("BonelabMultiplayerMockup", "Junction")]
