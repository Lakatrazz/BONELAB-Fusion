using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace SLZShaderInjector
{
    /*
    public class CodegenDebugWindow : EditorWindow
    {
        public ShaderInclude inputFile;
        public ShaderInclude outputFile;
        public SerializedObject thisObj;
        public SerializedProperty inputProp;
        public SerializedProperty outputProp;
        public SerializedProperty injectionProp;
        public ShaderInclude[] injectionFiles;
        public ShaderInjector cg;

        [MenuItem("Tools/LitMAS Codegen Debug")]
        static void Init()
        {
            CodegenDebugWindow window = (CodegenDebugWindow)GetWindow(typeof(CodegenDebugWindow));

            window.Show();
        }

        private void OnEnable()
        {
            cg = new ShaderInjector();
            thisObj = new SerializedObject(this);
            inputProp = thisObj.FindProperty("inputFile");
            outputProp = thisObj.FindProperty("outputFile");
            injectionProp = thisObj.FindProperty("injectionFiles");

        }

        private void OnGUI()
        {
            EditorGUILayout.ObjectField(inputProp);
            EditorGUILayout.ObjectField(outputProp);
            EditorGUILayout.PropertyField(injectionProp, true);
            thisObj.ApplyModifiedProperties();
            if (GUILayout.Button("Test"))
            {
                string appPath = Path.GetDirectoryName(Application.dataPath);
                cg.inputFileDir = Path.Combine(appPath, AssetDatabase.GetAssetPath(inputFile));

                cg.outputFileDir = Path.Combine(appPath, AssetDatabase.GetAssetPath(outputFile));
                cg.injectionDirs = new string[injectionFiles.Length];
                for (int i = 0; i < injectionFiles.Length; i++)
                {
                    cg.injectionDirs[i] = Path.Combine(appPath, AssetDatabase.GetAssetPath(injectionFiles[i]));
                }
                Debug.Log("Begin injection");
                cg.CreateShader();
            }
        }
    }
    */

    /// <summary>
    /// This script is a horrible cludge slapped together quickly to solve the issue of having to maintain many different
    /// versions of LitMAS, most of which is just duplicate code with minor variations. Goes through a set of specified include
    /// files looking for blocks of code contained in blocks guarded by //#!INJECT_BEGIN (name) (order) and //#!INJECT_END. It
    /// then looks for lines in the base file which begin with //!#INJECT_POINT (name) and pastes the code from all the blocks with
    /// the same at that point.
    /// </summary>
    public class ShaderInjector
    {
        public string outputFileDir;
        public string inputFileDir;
        public string[] injectionDirs;
        public Dictionary<string, int> TagIndex;
        public List<List<Tuple<int, string>>> injectionContent;
        public Dictionary<int, int> texcoordCounter;
        public string texcoord = "    {0} {1} : TEXCOORD{2};\n";
        public string blockHeader = "// Begin Injection {0} from {1} ----------------------------------------------------------\n";
        public string blockEnder = "// End Injection {0} from {1} ----------------------------------------------------------\n";


        const string warningHeader = "/*-----------------------------------------------------------------------------------------------------*\n" +
                                     " *-----------------------------------------------------------------------------------------------------*\n" +
                                     " * WARNING: THIS FILE WAS CREATED WITH SHADERINJECTOR, AND SHOULD NOT BE EDITED DIRECTLY. MODIFY THE   *\n" +
                                     " * BASE INCLUDE AND INJECTED FILES INSTEAD, AND REGENERATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   *\n" +
                                     " *-----------------------------------------------------------------------------------------------------*\n" +
                                     " *-----------------------------------------------------------------------------------------------------*/\n\n"
            ;


        public void CreateShader()
        {
            TagIndex = new Dictionary<string, int>();
            injectionContent = new List<List<Tuple<int, string>>>();
            texcoordCounter = new Dictionary<int, int>();
            foreach (string injDir in injectionDirs)
            {
                int status = ReadInjectionFile(injDir);
                if (status != 0) return;
            }
            sortInjectionContent();

            InjectBlocksIntoFile(inputFileDir, outputFileDir);
        }

        private int ReadInjectionFile(string injDir)
        {
            string file = "";
            if (ReadFile(injDir, out file) != 0)
            {
                return 1;
            }
            Debug.Log(file.Length);
            SILexer lexer = new SILexer();
            List<SILexer.CommandInfo> cmd = lexer.LexFile(ref file);
            if (cmd == null || cmd.Count == 0)
            {
                Debug.LogError("ShaderInjector: No injection commands in file at " + injDir);
                return 1;
            }
            SIParser parser = new SIParser(injDir);
            if (parser.validateInjection(cmd) != 0) //validation will handle printing errors
            {
                return 1;
            }
            string fileName = Path.GetFileName(injDir);
            readInjectionCommands(cmd, ref file, fileName);

            return 0;
        }

        private void readInjectionCommands(List<SILexer.CommandInfo> cmd, ref string file, string fileName)
        {
            for (int i = 0; i < cmd.Count; i++)
            {
                if (cmd[i].cmdType == SILexicon.CmdType.InjectBegin)
                {
                    string tag = cmd[i].parameters[0];
                    int order = Int32.Parse(cmd[i].parameters[1]); //parser should make sure this is an int

                    string block = ReadBlock(cmd, i, ref file, fileName);

                    if (!TagIndex.ContainsKey(tag))
                    {
                        TagIndex.Add(tag, TagIndex.Count);
                        injectionContent.Add(new List<Tuple<int, string>>());
                    }
                    Tuple<int, string> cmdTuple = new Tuple<int, string>(order, block);
                    injectionContent[TagIndex[tag]].Add(cmdTuple);
                }
            }
        }

        private string CreateTexcoord(SILexer.CommandInfo cmd)
        {
            int counter;
            int register = Int32.Parse(cmd.parameters[2]);
            if (texcoordCounter.TryGetValue(register, out counter))
            {
                counter = texcoordCounter[register] + 1;
                texcoordCounter[register] = counter;
            }
            else
            {
                counter = 0;
                texcoordCounter.Add(register, counter);
            }
            return String.Format(texcoord, cmd.parameters[0], cmd.parameters[1], counter);
        }
        private string ReadBlock(List<SILexer.CommandInfo> cmd, int index, ref string file, string fileName = null)
        {
            int i = index;
            StringBuilder block = new StringBuilder();
            if (fileName != null)
            {
                block.Append(string.Format(blockHeader, cmd[i].parameters[0], fileName));
            }
            int beginBlock = cmd[i].endIndex + 1;
            int j = i + 1;
            int endBlock;

            while (cmd[j].cmdType != SILexicon.CmdType.InjectEnd && j < cmd.Count)
            {

                endBlock = cmd[j].beginIndex;
                block.Append(file.Substring(beginBlock, Mathf.Max(endBlock - beginBlock, 0)));
                if (cmd[j].cmdType == SILexicon.CmdType.TexcoordCounter)
                {
                    beginBlock = cmd[j].beginIndex;
                }
                else
                {
                    beginBlock = cmd[j].endIndex + 1;
                }
                j++;
            }
            endBlock = cmd[j].beginIndex;
            block.Append(file.Substring(beginBlock, endBlock - beginBlock));
            if (fileName != null)
            {
                block.Append(string.Format(blockEnder, cmd[i].parameters[0], fileName));
            }
            return block.ToString();
        }
        private int ReadFile(string injDir, out string file)
        {
            file = "";
            try 
            {
                file = File.ReadAllText(injDir);
            } 
            catch (ArgumentException)
            {
                Debug.LogError("ShaderInjector: Invalid file directory: " + injDir);
                return 1;
            }
            catch (PathTooLongException)
            {
                Debug.LogError("ShaderInjector: File directory too long: " + injDir);
                return 1;
            }
            catch (FileNotFoundException)
            {
                Debug.LogError("ShaderInjector: File does not exist at path: " + injDir);
                return 1;
            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("ShaderInjector: Directory does not exist: " + injDir);
                return 1;
            }
            catch (IOException)
            {
                Debug.LogError("ShaderInjector: I/O error attempting to read file at: " + injDir);
                return 1;
            }

            return 0;
        }

     

        void sortInjectionContent()
        {
            foreach (var tagContent in injectionContent)
            {
                tagContent.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            }
        }

        void ParseSecondPass(ref StringBuilder sb, ref string file, ref List<SILexer.CommandInfo> cmd)
        {
            sb.Append(file.Substring(0, cmd[0].beginIndex));
            int i = 0;
            for (; i < cmd.Count; i++)
            {
                
                switch (cmd[i].cmdType)
                {
                    case SILexicon.CmdType.TexcoordCounter:
                        sb.Append(CreateTexcoord(cmd[i]));
                        Debug.Log(cmd[i].parameters[1]);
                        break;
                }
                int beginIndex = cmd[i].endIndex + 1;
                int endIndex = i != cmd.Count - 1 ? cmd[i + 1].beginIndex : file.Length;
                sb.Append(file.Substring(beginIndex, endIndex - beginIndex));
            }
        }

        int InjectBlocksIntoFile(string dirIn, string dirOut)
        {

            string file = "";
            if (ReadFile(dirIn, out file) != 0)
            {
                return 1;
            }
            SILexer lexer = new SILexer();
            List<SILexer.CommandInfo> cmd = lexer.LexFile(ref file);
            if (cmd == null || cmd.Count == 0)
            {
                Debug.LogError("ShaderInjector: No injection commands in file at " + dirIn);
                return 1;
            }
            SIParser parser = new SIParser(dirIn);
            if (parser.validateBase(cmd) != 0)
            {
                return 1;
            }

            StringBuilder OutputFile = new StringBuilder();
            OutputFile.Append(warningHeader);
            OutputFile.Append(file.Substring(0, cmd[0].beginIndex));
            int i = 0;
            int useDefault = 0;
            
            //bool skipBlock = false;
            for (; i < cmd.Count; i++)
            {
                switch (cmd[i].cmdType)
                {
                    case SILexicon.CmdType.InjectPoint:
                        useDefault = InjectBlock(cmd, i, ref OutputFile);
                        break;
                    case SILexicon.CmdType.InjectEnd:
                        break;
                    case SILexicon.CmdType.InjectDefault:
                        if (useDefault == 0)
                        {
                            ++i;
                            while (cmd[i].cmdType != SILexicon.CmdType.InjectEnd)
                            {
                                ++i;
                            }
                            useDefault = 0;
                        }

                        break;
                    case SILexicon.CmdType.TexcoordCounter:
                        OutputFile.Append(file.Substring(cmd[i].beginIndex, cmd[i].endIndex - cmd[i].beginIndex + 1));
                        break;
                }
                int beginIndex = cmd[i].endIndex + 1;
                int endIndex = i != cmd.Count - 1 ? cmd[i+1].beginIndex : file.Length;
                OutputFile.Append(file.Substring(beginIndex, endIndex - beginIndex));
            }

            //OutputFile.Append(file.Substring(0, cmd[0].beginIndex));
            string file2 = OutputFile.ToString();
            
            List<SILexer.CommandInfo> cmd2 = lexer.LexFile(ref file2);

            if (cmd2 != null && cmd2.Count > 0)
            {
                if (parser.validateBase(cmd2) != 0)
                {
                    return 1;
                }
                StringBuilder OutputFile2 = new StringBuilder();
                ParseSecondPass(ref OutputFile2, ref file2, ref cmd2);
                file2 = OutputFile2.ToString();
            }
            
            
            string assetPath = dirOut.Substring(Path.GetDirectoryName(Application.dataPath).Length + 1);
            ShaderInclude fileObj = AssetDatabase.LoadAssetAtPath<ShaderInclude>(assetPath);
            File.WriteAllText(dirOut, file2);
            EditorUtility.SetDirty(fileObj);
            //AssetDatabase.ImportAsset(assetPath);
            fileObj = null;
            return 0;
        }

        int InjectBlock(List<SILexer.CommandInfo> cmd, int index, ref StringBuilder outp)
        {
                string tag = cmd[index].parameters[0];
                if (!TagIndex.ContainsKey(tag))
                {
                    return 1;
                }
                int tagIndex = TagIndex[tag];
                foreach (Tuple<int, string> block in injectionContent[tagIndex])
                {
                    outp.Append(block.Item2);
                }
                return 0;
        }


        /*
        private int readInjectionFile(string injectionDir)
        {
            try
            {
                using (StreamReader inj = new StreamReader(injectionDir))
                {
                    string line;
                    bool isReadingInjection = false;
                    int lineNum = 0;
                    int currentTagIndex = -1;
                    int currentTagOrder = 0;
                    StringBuilder injectBlock = new StringBuilder();
                    while ((line = inj.ReadLine()) != null)
                    {
                        string lineClipped = line.TrimStart();
                        if (!isReadingInjection)
                        {
                            int injectTagStatus = ParseInjectBlock(lineClipped, lineNum, injectionDir, ref currentTagIndex, ref currentTagOrder);
                            switch (injectTagStatus)
                            {
                                case 0: break;
                                case 1: return 1;
                                case 2:
                                    isReadingInjection = true;
                                    break;
                            }
                        }
                        else
                        {
                            if (lineClipped.StartsWith(prefix))
                            {
                               
                                if (injectEnd.Equals(lineClipped.Substring(prefix.Length,injectEnd.Length)))
                                {
                                    injectionContent[currentTagIndex].Add(new Tuple<int, string>(currentTagOrder, injectBlock.ToString()));
                                    isReadingInjection = false;
                                    injectBlock.Clear();
                                }
                            }
                            else
                            {
                                injectBlock.AppendLine(line);
                            }
                        }
                        lineNum++;
                    }
                    injectBlock = null;
                    if (isReadingInjection)
                    {
                        Debug.LogError("Reached end of file while reading injection block");
                        return 1;
                    }
                }

                return 0;
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError(e.Message);
                return 1;
            }
        }

        int ParseInjectBlock(string line, int lineNum, string injectionDir, ref int currTagIndex, ref int currTagOrder)
        {
            if (line.StartsWith(prefix))
            {
                Debug.Log(line);
                if (injectBegin.Equals(line.Substring(prefix.Length,injectBegin.Length)))
                {
                    string[] words;
                    words = line.Split(new char[0], 4, System.StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length < 3)
                    {
                        Debug.LogError("Malformed injection tag at line " + lineNum + " in file " + injectionDir);
                        return 1;
                    }
                    try
                    {
                        currTagOrder = Int32.Parse(words[2]);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError("Malformed injection tag at line " + lineNum + " in file " + injectionDir +
                            ", could not parse " + words[2] + " as number \n" + e.Message);
                        return 1;
                    }

                    if (!TagIndex.ContainsKey(words[1]))
                    {
                        TagIndex.Add(words[1], TagIndex.Count);
                        injectionContent.Add(new List<Tuple<int, string>>());
                    }

                    currTagIndex = TagIndex[words[1]];
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        */
    }
}
