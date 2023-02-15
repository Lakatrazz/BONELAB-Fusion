using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SILexicon
{
    public enum CmdType : int
    {
        Invalid = -1,
        InjectPoint = 0,
        InjectDefault,
        InjectBegin,
        InjectEnd,
        TexcoordCounter
    }
    public struct InjectorCommand
    {
        public CmdType command;
        public int intParam;
        public int cmdStartIndex;
        public int cmdEndIndex;
        public string strParam;
    }

    public static string injectorPrefix = "//#!";

    public static Dictionary<string, CmdType> Lexicon = new Dictionary<string, CmdType>()
    {
        { "INJECT_POINT",   CmdType.InjectPoint   },
        { "INJECT_DEFAULT", CmdType.InjectDefault },
        { "INJECT_BEGIN",   CmdType.InjectBegin   },
        { "INJECT_END",     CmdType.InjectEnd     },
        { "TEXCOORD",     CmdType.TexcoordCounter },
    };

    public static Dictionary<CmdType, string> Lexicon2 = new Dictionary<CmdType, string>()
    {
        { CmdType.InjectPoint,  "INJECT_POINT"    },
        { CmdType.InjectDefault,"INJECT_DEFAULT"  },
        { CmdType.InjectBegin,  "INJECT_BEGIN"    },
        { CmdType.InjectEnd,    "INJECT_END"      },
        { CmdType.TexcoordCounter, "TEXCOORD"     }
    };

    static SILexicon s_Instance;

    public static SILexicon Instance { 
        get { 
            if (s_Instance == null) 
                s_Instance = new SILexicon(); 
            return s_Instance; 
        } }

    SILexicon()
    {
        generateTree();
    }
    class TreeNode
    {
        public char nodeChar;
        public CmdType command;
        public List<TreeNode> nextNode;
        public bool terminalNode;
    }

    TreeNode LexTreeHeadNode;

    void generateTree()
    {
        string[] cmdArray = new string[Lexicon.Count];
        Lexicon.Keys.CopyTo(cmdArray, 0);
        List<string> commands = new List<string>(cmdArray);
        LexTreeHeadNode = new TreeNode();
        populateNode(ref LexTreeHeadNode, commands, 0);
    }

    void populateNode(ref TreeNode node, List<string> cmds, int index)
    {
        List<char> nextChars = new List<char>();
        //Get list of strings with charact
        for (int i = 0; i < cmds.Count; i++)
        {
            if (index < cmds[i].Length)
            {
                char idxChar = cmds[i][index];
                if (!nextChars.Contains(idxChar))
                {
                    nextChars.Add(idxChar);
                }
            }
            else if (index == cmds[i].Length)
            {
                node.terminalNode = true;
                node.command = Lexicon[cmds[i]];
                //Debug.Log("Terminal Node: " + cmds[i] + " " + node.nodeChar);
            }
        }

        if (nextChars.Count > 0)
        {
            node.nextNode = new List<TreeNode>();
            int nextIndex = index + 1;
            foreach (char nextChar in nextChars)
            {
                TreeNode treeNode = new TreeNode();
                treeNode.nodeChar = nextChar;

                List<string> nextCmds = new List<string>();
                foreach (string currCmd in cmds)
                {
                    if (currCmd[index] == nextChar)
                    {
                       nextCmds.Add(currCmd);
                    }
                }
                populateNode(ref treeNode, nextCmds, nextIndex);
                node.nextNode.Add(treeNode);
            }
        }
    }

    public CmdType parseCommand(string word)
    {
        TreeNode CurrentNode = LexTreeHeadNode;
        TreeNode nextNode = null;
        for (int i = 0; i < word.Length; i++)
        {
            nextNode = null;
            if (CurrentNode.nextNode != null)
            {
                foreach (TreeNode node in CurrentNode.nextNode)
                {
                    if (node.nodeChar == word[i])
                    {
                        nextNode = node;
                        break;
                    }
                }
            }
            if (nextNode != null)
            {
                CurrentNode = nextNode;
            }
            else
            {
                //Debug.LogError("Command Longer than tree");
                return CmdType.Invalid;
            }
        }
        if (CurrentNode != null && CurrentNode.terminalNode)
        {
            //Debug.Log("Valid Command " + CurrentNode.command);
            return CurrentNode.command;
        }
        else
        {
            //Debug.LogError("Reached end of word without finding terminal node");
            return CmdType.Invalid;
        }
    }
}
