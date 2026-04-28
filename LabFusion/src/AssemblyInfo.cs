using System.Reflection;
using System.Runtime.CompilerServices;
using MelonLoader;

using LabFusion;

[assembly: AssemblyTitle(FusionMod.ModName)]
[assembly: AssemblyVersion(FusionVersion.VersionString)]
[assembly: AssemblyFileVersion(FusionVersion.VersionString)]
[assembly: InternalsVisibleTo("BonelabSupport")]

[assembly: MelonInfo(typeof(FusionMod), FusionMod.ModName, FusionVersion.VersionString, FusionMod.ModAuthor)]
[assembly: MelonGame(FusionMod.GameDeveloper)]
[assembly: MelonPriority(-10000)]
[assembly: MelonOptionalDependencies("Il2CppFacepunch.Steamworks.Win64", "Facepunch.Steamworks.Win64", "JNISharp")]