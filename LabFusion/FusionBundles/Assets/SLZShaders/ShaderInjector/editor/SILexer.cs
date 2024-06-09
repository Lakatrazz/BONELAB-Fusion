using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLZShaderInjector
{
    public class SILexer
    {
        string fileName;
        int index = 0;
        int lineNumber = 0;
        int lineStartIndex = 0;

        //public List<CommandInfo> commands;

        public class CommandInfo
        {
            public SILexicon.CmdType cmdType;
            public int line;
            public int beginIndex;
            public int endIndex;
            public List<string> parameters;
        }

        public List<CommandInfo> LexFile(ref string file)
        {
            List<CommandInfo> commands = new List<CommandInfo>();
            index = 0;
            lineNumber = 0;
            lineStartIndex = 0;
            while (!HasReachedEOF())
            {
                JumpToNextCommand(ref file);
                if (!HasReachedEOF())
                {
                    LexCommand(ref file, ref commands);
                }
            }
            /*
            foreach (CommandInfo cmd in commands)
            {
                string logOut = "Shader Injector Lexer Debug\n";
                logOut += "Command Type: " +cmd.cmdType + "\n";
                logOut += "Line: " + cmd.line + "\n";
                logOut += "Begin: " + cmd.beginIndex + "\n";
                logOut += "End: " + cmd.endIndex + "\n";
                logOut += "Parameters: \n";
                foreach (string pm in cmd.parameters)
                {
                    logOut += pm + "\n";
                }
                //Debug.Log(logOut);
            }
            */
            return commands;
        }
        void LexCommand(ref string file, ref List<CommandInfo> commands)
        {
            int nextWS = peekToWhiteSpace(ref file);
            SILexicon.CmdType command = SILexicon.Instance.parseCommand(file.Substring(index, nextWS - index));
            JumpToPeek(ref file, nextWS);
            CommandInfo cmd = new CommandInfo();
            cmd.cmdType = command;
            cmd.line = lineNumber;
            cmd.beginIndex = lineStartIndex;
            cmd.parameters = new List<string>();
            skipWhiteSpaceNoNewLine(ref file);
            while (!HasReachedEOF() && file[index] != '\n')
            {
                nextWS = peekToWhiteSpace(ref file);
                cmd.parameters.Add(file.Substring(index, nextWS - index));
                JumpToPeek(ref file, nextWS);
                if (!HasReachedEOF())
                {
                    skipWhiteSpaceNoNewLine(ref file);
                }
            }
            cmd.endIndex = HasReachedEOF() ? file.Length - 1 : index;
            commands.Add(cmd);
        }

        void JumpToNextCommand(ref string file)
        {
            if (HasReachedEOF())
            {
                return;
            }
            int end = file.Length - SILexicon.injectorPrefix.Length;
            while (index < end && index != -1)
            {
                skipWhiteSpace(ref file);
                if (index == -1)
                {
                    ReachedEOF();
                    return;
                }
                if (isHeader(ref file))
                {
                    index += SILexicon.injectorPrefix.Length;
                    return;
                }
                skipToNewLine(ref file);
            }
            ReachedEOF();
            return;
        }

        void skipToNewLine(ref string file)
        {
            if (HasReachedEOF())
            {
                return;
            }
            while (index < file.Length - 1)
            {
                if (file[index] == '\n')
                {
                    lineNumber++;
                    index++;
                    lineStartIndex = index;
                    return;
                }
                index++;
            }
            ReachedEOF();
            return;
        }


        void skipWhiteSpace(ref string file)
        {
            if (HasReachedEOF())
            {
                return;
            }
            while (index < file.Length)
            {
                if (!char.IsWhiteSpace(file[index]))
                {
                    return;
                }
                else if (file[index] == '\n')
                {
                    lineNumber++;
                    lineStartIndex = index + 1;
                }
                index++;
            }
            ReachedEOF();
        }

        void skipWhiteSpaceNoNewLine(ref string file)
        {
            if (HasReachedEOF())
            {
                return;
            }
            while (index < file.Length)
            {
                if (!char.IsWhiteSpace(file[index]) || file[index] == '\n')
                {
                    return;
                }
                index++;
            }
            ReachedEOF();
        }

        int peekToWhiteSpace(ref string file)
        {
            int peekIndex = index;
            while (peekIndex < file.Length)
            {
                if (char.IsWhiteSpace(file[peekIndex]))
                {
                    break;
                }
                peekIndex++;
            }
            return peekIndex;
        }

        void JumpToPeek(ref string file, int peekIndex)
        {
            if (peekIndex < file.Length)
            {
                index = peekIndex;
            }
            else
            {
                ReachedEOF();
            }
        }

        bool isHeader(ref string file)
        {
            for (int i = 0; i < SILexicon.injectorPrefix.Length; i++)
            {
                if (file[i + index] != SILexicon.injectorPrefix[i])
                {
                    return false;
                }
            }
            return true;
        }

        void ReachedEOF()
        {
            index = -1;
        }

        bool HasReachedEOF()
        {
            return index == -1;
        }
    }
}
