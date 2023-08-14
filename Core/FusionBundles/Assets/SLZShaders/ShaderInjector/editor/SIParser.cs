using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SLZShaderInjector
{
    public class SIParser
    {
        public string fileDir;
        public SIParser(string fileDir)
        {
            this.fileDir = fileDir;
        }
        enum ErrorCode : int
        {
            none,
            missingOrder,
            orderNotInt,
            missingTag,
            unexpectedParam,
            unexpectedCmd,
            expectedOtherCmd,
            unexpectedEnd,
        }

        public int validateBase(List<SILexer.CommandInfo> commands)
        {
            bool needsEnd = false;
            SILexicon.CmdType expectedCmd = SILexicon.CmdType.Invalid;
            SILexer.CommandInfo prevCmd = null;
            int prevCmdLine = 0;
            for (int i = 0; i < commands.Count; i++)
            {
                switch (commands[i].cmdType)
                {
                    case SILexicon.CmdType.Invalid:
                        Debug.LogError("Invalid ShaderInjector Command at line " + commands[i].line);
                        return -1;
                    case SILexicon.CmdType.InjectPoint:
                        ErrorCode cmdError0 = validateInjectPoint(commands[i], expectedCmd);
                        if (cmdError0 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError0, commands[i], expectedCmd);
                            return -1;
                        }
                        expectedCmd = SILexicon.CmdType.Invalid;
                        break;
                    case SILexicon.CmdType.InjectDefault:
                        ErrorCode cmdError1 = validateInjectDefault(commands[i], prevCmd, expectedCmd);
                        if (cmdError1 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError1, commands[i], expectedCmd);
                            return -1;
                        }
                        expectedCmd = SILexicon.CmdType.InjectEnd;
                        break;
                    case SILexicon.CmdType.InjectEnd:
                        ErrorCode cmdError2 = validateInjectEnd(commands[i], expectedCmd);
                        if (cmdError2 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError2, commands[i], expectedCmd);
                            return -1;
                        }
                        expectedCmd = SILexicon.CmdType.Invalid;
                        break;
                    case SILexicon.CmdType.TexcoordCounter:
                        ErrorCode cmdError3 = validateTexcoord(commands[i]);
                        if (cmdError3 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError3, commands[i], expectedCmd);
                            return -1;
                        }
                        break;
                    default:
                        PrintErrorMessage(ErrorCode.unexpectedCmd, commands[i], expectedCmd);
                        return -1;
                }
                prevCmd = commands[i];
            }
            if (expectedCmd != SILexicon.CmdType.Invalid)
            {
                PrintErrorMessage(ErrorCode.unexpectedEnd, commands[commands.Count - 1], expectedCmd);
                return -1;
            }
            return 0;
        }

        public int validateInjection(List<SILexer.CommandInfo> commands)
        {
            bool needsEnd = false;
            SILexicon.CmdType expectedCmd = SILexicon.CmdType.Invalid;
            SILexer.CommandInfo prevCmd = null;
            int prevCmdLine = 0;
            for (int i = 0; i < commands.Count; i++)
            {
                switch (commands[i].cmdType)
                {
                    case SILexicon.CmdType.Invalid:
                        Debug.LogError("Invalid ShaderInjector Command at line " + commands[i].line);
                        return -1;
                    case SILexicon.CmdType.InjectBegin:
                        ErrorCode cmdError0 = validateInjectBegin(commands[i], expectedCmd);
                        if (cmdError0 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError0, commands[i], expectedCmd);
                            return -1;
                        }
                        expectedCmd = SILexicon.CmdType.InjectEnd;
                        break;
                    case SILexicon.CmdType.InjectEnd:
                        ErrorCode cmdError1 = validateInjectEnd(commands[i], expectedCmd);
                        if (cmdError1 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError1, commands[i], expectedCmd);
                            return -1;
                        }
                        expectedCmd = SILexicon.CmdType.Invalid;
                        break;
                    case SILexicon.CmdType.TexcoordCounter:
                        ErrorCode cmdError2 = validateTexcoord(commands[i]);
                        if (cmdError2 != ErrorCode.none)
                        {
                            PrintErrorMessage(cmdError2, commands[i], expectedCmd);
                            return -1;
                        }
                        break;
                    default:
                        PrintErrorMessage(ErrorCode.unexpectedCmd, commands[i], expectedCmd);
                        return -1;
                }
                prevCmd = commands[i];
            }
            if (expectedCmd != SILexicon.CmdType.Invalid)
            {
                PrintErrorMessage(ErrorCode.unexpectedEnd, commands[commands.Count - 1], expectedCmd);
                return -1;
            }
            return 0;
        }


        ErrorCode validateInjectEnd(SILexer.CommandInfo cmd, SILexicon.CmdType expectedCmd)
        {
            if (expectedCmd != SILexicon.CmdType.InjectEnd)
            {
                return ErrorCode.unexpectedCmd;
            }
            if (cmd.parameters.Count > 0)
            {
                return ErrorCode.unexpectedParam;
            }
            return ErrorCode.none;
        }


        ErrorCode validateInjectBegin(SILexer.CommandInfo cmd, SILexicon.CmdType expectedCmd)
        {
            if (expectedCmd != SILexicon.CmdType.Invalid && cmd.cmdType != expectedCmd )
            {
                return ErrorCode.expectedOtherCmd;
            }
            if (cmd.parameters.Count < 1)
            {
                return ErrorCode.missingTag;
            }
            if (cmd.parameters.Count == 1)
            {
                return ErrorCode.missingOrder;
            }
            int param2 = 0;
            if (!Int32.TryParse(cmd.parameters[1], out param2))
            {
                return ErrorCode.orderNotInt;
            }
            return ErrorCode.none;
        }

        ErrorCode validateInjectPoint(SILexer.CommandInfo cmd, SILexicon.CmdType expectedCmd)
        {
            if (expectedCmd != SILexicon.CmdType.Invalid && cmd.cmdType != expectedCmd)
            {
                return ErrorCode.expectedOtherCmd;
            }
            if (cmd.parameters.Count == 0)
            {
                return ErrorCode.missingTag;
            }
            if (cmd.parameters.Count > 1)
            {
                return ErrorCode.unexpectedParam;
            }
            return ErrorCode.none;
        }

        ErrorCode validateInjectDefault(SILexer.CommandInfo cmd, SILexer.CommandInfo prevCmd, SILexicon.CmdType expectedCmd)
        {
            if (expectedCmd != SILexicon.CmdType.Invalid && cmd.cmdType != expectedCmd)
            {
                return ErrorCode.expectedOtherCmd;
            }
            if (prevCmd.cmdType != SILexicon.CmdType.InjectPoint)
            {
                return ErrorCode.unexpectedCmd;
            }
            if (cmd.parameters.Count > 0)
            {
                return ErrorCode.unexpectedParam;
            }
            return ErrorCode.none;
        }

        ErrorCode validateTexcoord(SILexer.CommandInfo cmd)
        {
            if (cmd.parameters.Count < 2)
            {
                return ErrorCode.missingTag;
            }
            if (cmd.parameters.Count == 2)
            {
                return ErrorCode.missingOrder;
            }
            if (cmd.parameters.Count > 3)
            {
                return ErrorCode.unexpectedParam;
            }
            int param2 = 0;
            if (!Int32.TryParse(cmd.parameters[2], out param2))
            {
                return ErrorCode.orderNotInt;
            }
            return ErrorCode.none;
        }
        void PrintErrorMessage(ErrorCode error, SILexer.CommandInfo curr, SILexicon.CmdType expected)
        {
            string currCmdName = SILexicon.Lexicon2[curr.cmdType];
            int currLine = curr.line;
            string expectedCmdName = expected != SILexicon.CmdType.Invalid ? SILexicon.Lexicon2[expected] : "INVALID_TYPE";
            switch (error)
            {
                case ErrorCode.missingOrder:
                    Debug.LogError(string.Format("ShaderInjector: {0} at line {1} is missing order parameter\n {2}", currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.orderNotInt:
                    Debug.LogError(string.Format("ShaderInjector: {0} at line {1} cannot convert parameter to int\n {2}", currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.missingTag:
                    Debug.LogError(string.Format("ShaderInjector: {0} at line {1} is missing tag name\n {2}", currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.unexpectedParam:
                    Debug.LogError(string.Format("ShaderInjector: {0} at line {1} has an unexpected parameter\n {2}", currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.expectedOtherCmd:
                    Debug.LogError(string.Format("ShaderInjector: expected {0}, found {1} at line {2}\n {3}", expectedCmdName, currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.unexpectedCmd:
                    Debug.LogError(string.Format("ShaderInjector: Unexpected command {0} at line {1}\n {2}", currCmdName, currLine, fileDir));
                    break;
                case ErrorCode.unexpectedEnd:
                    Debug.LogError(string.Format("ShaderInjector: Reached end of file before finding {2} to match {0} at line {1} \n {3}", currCmdName, currLine, expectedCmdName, fileDir));
                    break;
                default:
                    Debug.LogError(string.Format("ShaderInjector: unknown error in {0} at line {1} \n {2}", currCmdName, currLine, fileDir));
                    break;
            }
        }
    }
}
