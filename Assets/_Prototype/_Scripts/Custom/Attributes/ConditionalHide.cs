using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                AttributeTargets.Class | AttributeTargets.Struct)]
public class ConditionalHideAttribute : PropertyAttribute
{
    //The name of the bool field that will be in control
    public string ConditionalSourceField;
    //TRUE = True = hide / false = unhide
    public bool IsInverted;
    // if it is an event that needs to be drawn
    public bool IsEvent;
 
    public ConditionalHideAttribute(string conditionalSourceField)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.IsInverted = false;
    }
 
    public ConditionalHideAttribute(string conditionalSourceField, bool isInverted)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.IsInverted = isInverted;
    }

    public ConditionalHideAttribute(string conditionalSourceField, bool isInverted, bool isEvent)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.IsInverted = isInverted;
        this.IsEvent = isEvent;
    }
}