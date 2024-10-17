using LabFusion.Utilities;

namespace LabFusion.SDK.Modules;

/// <summary>
    /// Logger for Fusion modules.
    /// </summary>
public class ModuleLogger
{
    private string _moduleName;

    public ModuleLogger(string moduleName)
    {
        _moduleName = moduleName;
    }

    private string ParseTxt(string txt) => $"-> [{_moduleName}] {txt}";

    public void Log(string txt, ConsoleColor color = ConsoleColor.White) => FusionLogger.Log(ParseTxt(txt), color);

    public void Warn(string txt) => FusionLogger.Warn(ParseTxt(txt));

    public void Error(string txt) => FusionLogger.Error(ParseTxt(txt));

    public void LogException(string task, Exception e) => FusionLogger.LogException(task, e);
}