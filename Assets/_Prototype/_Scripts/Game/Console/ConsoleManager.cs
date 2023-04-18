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
    
    public static bool consoleInitialized = false;
    
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
        chatGPT._initialPrompt = consoleSettings.GPTInitialPrompt;
        //Enable ChatGPT
        // chatGPT.Init();
        // chatLog = $"{consoleName} Console initialized.";
        
        chatLog = $"{consoleName} Console not initialized. Command mode only.";
    }

    void Update()
    {
		if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (consoleInput.text == "") return;
            // add input into chatlog
            chatLog += "\n" + inputName + " " + consoleInput.text;
            // clear input field
            if (!consoleCommandManager.CheckConsoleCommand(consoleInput.text) && consoleInitialized) SubmitChatMessage();
            consoleInput.text = "";
        }
        // update chat log
        consoleOutput.text = chatLog;
        // clean up chat log
        if (chatLog.Length > 1000) chatLog = chatLog.Substring(chatLog.Length - 1000);
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
            chatLog += $"\n{consoleName} Error. Please reboot console. Use => <color=#e32954>/reboot</color>";
        }
    }

    private void SubmitChatMessage()
    {
        if (consoleInput.text == "") return;
        chatGPT.SendToChatGPT("{\"consoleInput\":\"" + consoleInput.text + "\"}");
    }

    IEnumerator AwaitingResponseRoutine(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
    }
}
