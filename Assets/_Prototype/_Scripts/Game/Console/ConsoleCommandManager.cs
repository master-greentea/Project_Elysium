using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ChatGPTWrapper;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public class ConsoleCommandManager : MonoBehaviour
{ 
    private ChatGPTConversation chatGPT;
    [SerializeField] private ConsoleCommands[] consoleCommands;
    
    private void Awake()
    {
        chatGPT = GetComponent<ChatGPTConversation>();
    }

    /// <summary>
    /// Check if a console command has been triggered or not
    /// </summary>
    /// <param name="input">Player input string</param>
    /// <returns></returns>
    public bool CheckConsoleCommand(string input)
    {
        if (input == "") return false;
        input = input.ToLower();
        if (input[0] != '/') return false;
        // handle commands
        string currentCommandString = input.Contains("(") ? input.Split('(')[0].Substring(1) : input.Substring(1);
        foreach (var cc in consoleCommands)
        {
            // ignore if command is not active or not matched with a command string
            if (!cc.isActive || cc.commandString != currentCommandString) continue;
            // for commands with parameters
            if (cc.hasParameter)
            {
                try
                {
                    string parameters = input.Split('(')[1].Split(')')[0];
                    cc.triggerEventWithParameter.Invoke(parameters);
                    return true;
                }
                catch (Exception e)
                {
                    cc.triggerEvent.Invoke();
                    return true;
                }
            }
            cc.triggerEvent.Invoke();
            return true;
        }
        // return true if no command was found, but print a console response first
        ConsoleManager.chatLog += $"\n{ConsoleManager.consoleName} Console command not found.";
        return true;
    }

    // event functions
    public void ConsoleCommand_Clear()
    {
        ConsoleManager.chatLog = $"{ConsoleManager.consoleName} Console cleared.";
    }

    public void ConsoleCommand_Reboot()
    {
        StartCoroutine(ConsoleCommandRoutine_Reboot());
        chatGPT.ResetChat(chatGPT._initialPrompt);
    }
    
    IEnumerator ConsoleCommandRoutine_Reboot()
    {
        ConsoleManager.chatLog = $"{ConsoleManager.consoleName} Console rebooting...";
        yield return new WaitForSecondsRealtime(.5f);
        ConsoleManager.chatLog = $"{ConsoleManager.consoleName} Console rebooting..." +
                                 $"\n{ConsoleManager.consoleName} Console rebooted.";
    }
    
    public void ConsoleCommand_Init(string parameters)
    {
        chatGPT._apiKey = parameters;
        chatGPT.Init();
        ConsoleManager.chatLog += $"\n{ConsoleManager.consoleName} Console initialized with API key: {parameters}";
    }
    
    public void ConsoleCommand_Init()
    {
        ConsoleManager.chatLog += $"\n{ConsoleManager.consoleName} Console command requires API key. Use => /init(API Key)";
    }
}

[Serializable]
public struct ConsoleCommands
{
    public bool isActive;
    public string commandString;
    public UnityEvent triggerEvent;
    public bool hasParameter;
    public UnityEvent<string> triggerEventWithParameter;
}
