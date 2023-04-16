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
    [SerializeField] private ConsoleSettings consoleSettings;
    [Header("Console UI")]
    [SerializeField] public TMP_InputField consoleInput;
    [SerializeField] private TextMeshProUGUI consoleOutput;
    
    private string npcName;
    private string inputName;
    
    private string chatLog = "";
    
    void Awake()
    {
        Services.ConsoleManager = this;
        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        chatLog += consoleOutput.text;
        npcName = consoleSettings.consoleName;
        inputName = consoleSettings.inputName;
        chatGPT._initialPrompt = consoleSettings.gptInitialPrompt;
        Debug.Log(chatGPT._initialPrompt);
        //Enable ChatGPT
        chatGPT.Init();
    }

    void Update()
    {
		if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!CheckConsoleCommands()) SubmitChatMessage();
            consoleInput.text = "";
        }
        // update chat log
        consoleOutput.text = chatLog;
    }

    bool CheckConsoleCommands()
    {
        consoleInput.text = consoleInput.text.ToLower();
        switch (consoleInput.text)
        {
            case "/clear":
                chatLog = "// Console cleared.";
                return true;
            case "/reboot":
                StartCoroutine(ConsoleReboot());
                chatGPT.ResetChat(chatGPT._initialPrompt);
                return true;
        }
        return false;
    }

    IEnumerator ConsoleReboot()
    {
        chatLog = "// Console rebooting...";
        yield return new WaitForSecondsRealtime(.5f);
        chatLog = "// Console rebooting...\n// Console rebooted.";
    }

    public void ReceiveChatGPTReply(string message)
    {
        print(message);
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
            chatLog += "<br>" + npcName + " " + responseLine;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            chatLog += "<br>" + npcName + " " + "Error. Please reboot console.";
        }
    }

    public void SubmitChatMessage()
    {
        if (consoleInput.text == "") return;
        Debug.Log("Message sent: " + consoleInput.text);
        chatGPT.SendToChatGPT("{\"consoleInput\":\"" + consoleInput.text + "\"}");
        chatLog += "<br>" + inputName + " " + consoleInput.text;
        // clear input field
        consoleInput.text = "";
    }
}
