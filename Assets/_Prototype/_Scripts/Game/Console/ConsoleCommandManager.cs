using System;
using System.Collections;
using System.Collections.Generic;
using ChatGPTWrapper;
using UnityEngine;
using UnityEngine.Events;

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
        input = input.ToLower();
        if (input[0] != '/') return false;
        // handle commands
        string currentCommandString = input.Substring(1);
        foreach (var cc in consoleCommands)
        {
            // ignore if command is not active
            if (!cc.isActive) continue;
            // invoke event if command matches
            if (cc.commandString == currentCommandString)
            {
                cc.triggerEvent.Invoke();
                return true;
            }
        }
        // return true if no command was found, but print a console response first
        ConsoleManager.chatLog += $"\n{ConsoleManager.inputName} {input}" +
                                  $"\n{ConsoleManager.consoleName} Console command not found.";
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
}

[Serializable]
public struct ConsoleCommands
{
    public bool isActive;
    public string commandString;
    public UnityEvent triggerEvent;
}
