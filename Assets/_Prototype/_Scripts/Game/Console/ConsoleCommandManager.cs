using System;
using System.Collections;
using ChatGPTWrapper;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ConsoleCommandManager : MonoBehaviour
{ 
    private ChatGPTConversation chatGPT;
    [SerializeField] private ConsoleCommand[] consoleCommands;
    [SerializeField] private TMP_InputField apiInputField;

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
        input = input.TrimEnd();
        if (input[0] != '/') return false;
        // handle commands
        string currentCommandString = input.Contains(' ') ? input.Split(' ')[0].Substring(1) : input.Substring(1);
        currentCommandString = currentCommandString.ToLower();
        foreach (var cc in consoleCommands)
        {
            // ignore if command is not active or not matched with a command string
            if (!cc.isActive || cc.CommandString != currentCommandString) continue;
            // for commands with parameters
            if (cc.hasParameter)
            {
                try
                {   
                    string parameters = input.Split(' ')[1];
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
        ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} Console command not found.";
        return true;
    }

    // BELOW: event functions
    /// <summary>
    /// Clear
    /// </summary>
    public void ConsoleCommand_Clear()
    {
        ConsoleMenuManager.chatLog = $"{ConsoleMenuManager.consoleName} Console cleared.";
    }

    /// <summary>
    /// Reboot
    /// </summary>
    public void ConsoleCommand_Reboot()
    {
        StartCoroutine(ConsoleCommandRoutine_Reboot());
        chatGPT.ResetChat(chatGPT._initialPrompt);
    }
    IEnumerator ConsoleCommandRoutine_Reboot()
    {
        ConsoleMenuManager.chatLog = $"{ConsoleMenuManager.consoleName} Console rebooting...";
        yield return new WaitForSecondsRealtime(.5f);
        ConsoleMenuManager.chatLog = $"{ConsoleMenuManager.consoleName} Console rebooting..." +
                                 $"\n{ConsoleMenuManager.consoleName} Console rebooted.";
    }
    
    /// <summary>
    /// Init API
    /// </summary>
    /// <param name="parameters">API</param>
    public void ConsoleCommand_Init(string parameters)
    {
        // check if already initialized
        if (Services.ConsoleMenuManager.consoleInitialized)
        {
            ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} Console already initialized.";
            return;
        }
        chatGPT._apiKey = parameters;
        chatGPT.Init();
        ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} Console initialized.";
    }
    
    /// <summary>
    /// Init
    /// </summary>
    public void ConsoleCommand_Init()
    {
        // check if already initialized
        if (Services.ConsoleMenuManager.consoleInitialized)
        {
            ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} Console already initialized.";
            return;
        }
        if (apiInputField.text == "") 
        {
            ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} API not found. " +
                                  "Use => <color=#e32954>/init APIKey</color>";
            return;
        }
        chatGPT._apiKey = apiInputField.text;
        chatGPT.Init();
        ConsoleMenuManager.chatLog += $"\n{ConsoleMenuManager.consoleName} Console initialized.";
    }
}

public enum ConsoleCommands{
    Clear, Reboot, Init,
}

[Serializable]
public class ConsoleCommand
{
    public ConsoleCommands commandType;
    public string CommandString
    {
        get
        {
            return commandType switch
            {
                ConsoleCommands.Clear => "clear",
                ConsoleCommands.Reboot => "reboot",
                ConsoleCommands.Init => "init",
            };
        }
    }
    public bool isActive;
    public bool hasParameter;
    // events
    public UnityEvent triggerEvent;
    [ConditionalHide("hasParameter", false, true)]
    public UnityEvent<string> triggerEventWithParameter;
}
