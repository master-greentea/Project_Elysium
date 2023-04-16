using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Console Settings", menuName = "ScriptableObjects/GameSettings")]
public class ConsoleSettings : ScriptableObject
{
    public string consoleName;
    public string inputName;
    [Header("GPT")]
    public string apiKey;
    [TextArea(4, 200)] public string personalitySet;
    public string gptInitialPrompt => personalitySet + "\nPlayer input JSON format: " + inputJSONFormat + "\nYour output JSON format: " + outputJSONFormat;
    [SerializeField] private string inputJSONFormat;
    [SerializeField] private string outputJSONFormat;
}