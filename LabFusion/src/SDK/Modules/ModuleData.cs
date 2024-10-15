namespace LabFusion.SDK.Modules;

/// <summary>
/// Data containing the outline for a module to be loaded.
/// </summary>
public sealed class ModuleData
{
    public Type ModuleType { get; set; } = null;

    public string Name { get; set; } = null;
    public string Author { get; set; } = "Unknown";
    public string Version { get; set; } = "0.0.0";

    public ConsoleColor Color { get; set; } = ConsoleColor.Magenta;
}