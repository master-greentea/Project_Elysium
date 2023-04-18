using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Console Settings", menuName = "Console/Settings")]
public class ConsoleSettings : ScriptableObject
{
    [Header("Console Settings")] public string consoleName;
    public string inputName;

    [Header("Initial Prompt")]
    [TextArea(4, 200)] public string personalityPrompt;
    [TextArea(2, 4)] public string JSONFormatPrompt;
    [SerializeField] private string inputJSONFormat;
    [SerializeField] private string outputJSONFormat;
    
    [Header("Concepts")]
    public ConsoleConceptDatabase[] conceptDatabases;
    
    public string GPTInitialPrompt
    {
        get
        {
            // initial prompt
            string prompt = $"{personalityPrompt}\nBelow are some concepts that you know about this world.";
            foreach (var conceptDb in conceptDatabases)
            {
                if (!conceptDb.isActive) continue;
                foreach (var concept in conceptDb.conceptList)
                    // add concepts into prompt
                    prompt += $"{concept.concept}\n";
            }
            // output json format
            prompt += $"{JSONFormatPrompt}\nPlayer input JSON format: {inputJSONFormat}\nYour output JSON format: {outputJSONFormat}";
            return prompt;
        }
    }
}
