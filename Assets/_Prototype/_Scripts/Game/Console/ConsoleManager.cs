using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ChatGPTWrapper;
using System;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ConsoleManager : MonoBehaviour
{
    public static Canvas canvas;
    [Header("GPT Settings")]
    [SerializeField] private ChatGPTConversation chatGPT;
    private ConsoleCommandManager consoleCommandManager;
    [SerializeField] private ConsoleSettings consoleSettings;
    [Header("Console UI")]
    [SerializeField] public TMP_InputField consoleInput;
    [SerializeField] private TextMeshProUGUI consoleOutput;
    
    public static string consoleName;
    public static string inputName;
    
    public static string chatLog = "";
    
    void Awake()
    {
        Services.ConsoleManager = this;
        consoleCommandManager = chatGPT.GetComponent<ConsoleCommandManager>();
        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        chatLog += consoleOutput.text;
        consoleName = consoleSettings.consoleName;
        inputName = consoleSettings.inputName;
        chatGPT._initialPrompt = consoleSettings.gptInitialPrompt;
        //Enable ChatGPT
        chatGPT.Init();
        chatLog = $"{consoleName} Console initialized.";
    }

    void Update()
    {
		if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!consoleCommandManager.CheckConsoleCommand(consoleInput.text)) SubmitChatMessage();
            consoleInput.text = "";
        }
        // update chat log
        consoleOutput.text = chatLog;
    }

    public void ReceiveChatGPTReply(string message)
    {
        try
        {
            if (!message.EndsWith("}"))
            {
                if (message.Contains("}"))
                {
                    message = message.Substring(0, message.LastIndexOf("}") + 1);
                }
                else
                {
                    message += "}";
                }
            }
            ConsoleJSONReceiver npcJSON = JsonUtility.FromJson<ConsoleJSONReceiver>(message);
            string responseLine = npcJSON.consoleReply;
            chatLog += $"\n{consoleName} {responseLine}";
        }
        catch (Exception e)
        {
            Debug.Log(message);
            chatLog += $"\n{consoleName} Error. Please reboot console. Use => /reboot";
        }
    }

    private void SubmitChatMessage()
    {
        if (consoleInput.text == "") return;
        chatGPT.SendToChatGPT("{\"consoleInput\":\"" + consoleInput.text + "\"}");
        // add input into chatlog
        chatLog += "\n" + inputName + " " + consoleInput.text;
        // clear input field
        consoleInput.text = "";
        // TODO: start response coroutine
    }

    IEnumerator AwaitingResponseRoutine(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
    }
}
