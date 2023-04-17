using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Console Settings", menuName = "ScriptableObjects/GameSettings")]
public class ConsoleSettings : ScriptableObject
{
    [Header("Console Settings")] public string consoleName;
    public string inputName;

    [Header("Initial Prompt")]
    [TextArea(4, 200)] public string personalityPrompt;
    public ConsoleConcepts[] conceptPrompts;
    [TextArea(2, 4)] public string JSONFormatPrompt;
    [SerializeField] private string inputJSONFormat;
    [SerializeField] private string outputJSONFormat;
    
    public string gptInitialPrompt
    {
        get
        {
            // initial prompt
            string prompt = $"{personalityPrompt}\nBelow are some concepts that you know about this world.";
            foreach (var concept in conceptPrompts)
            {
                // add concepts into prompt
                prompt += $"{concept.concept}\n";
            }
            // output json format
            prompt += $"{JSONFormatPrompt}\nPlayer input JSON format: {inputJSONFormat}\nYour output JSON format: {outputJSONFormat}";
            return prompt;
        }
    }
}

[Serializable]
public struct ConsoleConcepts
{
    public string conceptName;
    [TextArea(4, 50)] public string conceptDetails;
    public string concept => $"{conceptName}: {conceptDetails}";
}
