using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    private UnityEventDrawer eventDrawer;
    private bool isEvent;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //get the attribute data
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        //check if the propery we want to draw should be enabled
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
        //check if is event to use event specific drawing
        isEvent = condHAtt.IsEvent;
 
        //Enable/disable the property
        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;

        eventDrawer ??= new UnityEventDrawer();
 
        //Check if we should draw the property
        if (enabled)
        {
            if (isEvent) eventDrawer.OnGUI(position, property, label);
            else EditorGUI.PropertyField(position, property, label, true);
        }
 
        //Ensure that the next property that is being drawn uses the correct settings
        GUI.enabled = wasEnabled;
    }
    
    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = true;
        //Look for the sourcefield within the object that the property belongs to
        string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
        string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
 
        if (sourcePropertyValue != null)
        {
            enabled = sourcePropertyValue.boolValue;
        }
        else
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }

        // invert condition
        if (condHAtt.IsInverted) enabled = !enabled;
        return enabled;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
 
        if (enabled)
        {
            if (isEvent) return eventDrawer.GetPropertyHeight(property, label);
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            //The property is not being drawn
            //We want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
