using System.Reflection;
using System.Resources;

using LabFusionUpdater;

using MelonLoader;

[assembly: AssemblyTitle(FusionUpdaterPlugin.Name)]
[assembly: AssemblyProduct(FusionUpdaterPlugin.Name)]
[assembly: AssemblyCopyright("Created by " + FusionUpdaterPlugin.Author)]
[assembly: AssemblyVersion(FusionUpdaterPlugin.Version)]
[assembly: AssemblyFileVersion(FusionUpdaterPlugin.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(FusionUpdaterPlugin), FusionUpdaterPlugin.Name, FusionUpdaterPlugin.Version, FusionUpdaterPlugin.Author, null)]

[assembly: MelonGame("Stress Level Zero", "BONELAB")]