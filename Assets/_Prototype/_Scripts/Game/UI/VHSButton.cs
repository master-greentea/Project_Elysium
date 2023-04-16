using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler
{
    private EventSystem EventSystem;
    public TextMeshProUGUI tmp;
    [HideInInspector] public string buttonText;
    public VHSButtons buttonId;
    [HideInInspector] public Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        EventSystem = EventSystem.current;
        buttonText = buttonId switch
        {
            VHSButtons.Resume => "Resume",
            VHSButtons.Rewind => "Rewind",
            VHSButtons.Settings => "Settings",
            VHSButtons.Eject => "Eject",
            VHSButtons.Back => "Back",
            VHSButtons.InvertCamera => Services.PlayerController.isInvertedControls ? "Invert Camera: ON" : "Invert Camera: OFF",
            VHSButtons.SetTime => "Set Timestamp",
            VHSButtons.ConfirmTime => "Confirm",
            VHSButtons.CancelTime => "Cancel",
            VHSButtons.ConsoleResume => "Resume",
        };
        SetText(buttonText);
    }

    void Update()
    {
        if (gameObject == EventSystem.currentSelectedGameObject)
        {
           SetText("> " + buttonText);
        }
        else SetText(buttonText);
        // button activation visual
        tmp.color = button.enabled ? Color.white : Color.black;
    }
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (button.enabled) Services.PauseManager.SetButtonSelected(buttonId);
    }

    /// <summary>
    /// Set the text on the button to a specific phrase
    /// </summary>
    /// <param name="text">Text to set to</param>
    private void SetText(string text)
    {
        tmp.text = text;
    }
}
