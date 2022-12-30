using System;
using System.Diagnostics;
using System.Reflection;

using LabFusion.Utilities;

namespace LabFusion.SDK.Modules {
    /// <summary>
    /// Logger for Fusion modules.
    /// </summary>
    public class ModuleLogger {
        internal string _moduleName;

        public ModuleLogger(string moduleName) {
            _moduleName = moduleName;
        }

        internal string Internal_ParseTxt(string txt) => $"-> [{_moduleName}] {txt}";

        public void Log(string txt, ConsoleColor color = ConsoleColor.White) => FusionLogger.Log(Internal_ParseTxt(txt), color);

        public void Warn(string txt) => FusionLogger.Warn(Internal_ParseTxt(txt));

        public void Error(string txt) => FusionLogger.Error(Internal_ParseTxt(txt));

        public void LogException(string task, Exception e) => FusionLogger.LogException(task, e);
    }
}
