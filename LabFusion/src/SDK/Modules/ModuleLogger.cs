using LabFusion.Utilities;

namespace LabFusion.SDK.Modules;

/// <summary>
/// Logger for Fusion modules.
/// </summary>
public class ModuleLogger
{
    private readonly string _moduleName;

    public ModuleLogger(string moduleName)
    {
        _moduleName = moduleName;
    }

    private string Parse(string text) => $"-> [{_moduleName}] {text}";

    public void Log(string text, ConsoleColor color = ConsoleColor.White) => FusionLogger.Log(Parse(text), color);

    public void Warn(string text) => FusionLogger.Warn(Parse(text));

    public void Error(string text) => FusionLogger.Error(Parse(text));

    public void LogException(string task, Exception e) => FusionLogger.LogException(task, e);
}