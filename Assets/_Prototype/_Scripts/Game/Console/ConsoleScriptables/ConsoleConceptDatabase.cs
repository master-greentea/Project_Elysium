using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ConsoleDB", menuName = "Console/Concept Database")]
public class ConsoleConceptDatabase : ScriptableObject
{
    public bool isActive;
    public ConsoleConcepts[] conceptList;
}

[Serializable]
public struct ConsoleConcepts
{
    public string conceptName;
    [TextArea(4, 50)] public string conceptDetails;
    public string concept => $"{conceptName}: {conceptDetails}";
}
