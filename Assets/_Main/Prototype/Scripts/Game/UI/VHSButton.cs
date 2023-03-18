using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EventSystem EventSystem;
    public TextMeshProUGUI tmp;
    private string buttonText;
    public VHSButtons buttonId;
    [HideInInspector] public Button button;
    [HideInInspector] public bool isMouseOver;

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
            VHSButtons.CancelTime => "Cancel"
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
        isMouseOver = true;
        if (button.enabled) Services.VHSButtonsManager.SetButtonSelected(buttonId);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        isMouseOver = false;
    }

    /// <summary>
    /// Set the text on the button to a specific phrase
    /// </summary>
    /// <param name="text">Text to set to</param>
    private void SetText(string text)
    {
        tmp.text = text;
    }
    
    // menu functions
    public void Resume()
    {
        Services.GameManager.TogglePause();
    }

    public void Rewind()
    {
        Services.VHSButtonsManager.SwitchButtonSet("Rewind");
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Back);
    }

    public void Settings()
    {
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Settings);
        Services.VHSButtonsManager.SwitchButtonSet("Settings");
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Back);
    }

    public void Eject()
    {
        Application.Quit();
    }
    
    // settings functions
    public void BackFromSettings()
    {
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Paused);
        Services.VHSButtonsManager.SwitchButtonSet("Menu");
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Settings);
    }

    public void InvertCamera()
    {
        Services.PlayerController.isInvertedControls = !Services.PlayerController.isInvertedControls;
        buttonText = Services.PlayerController.isInvertedControls ? "Invert Camera: ON" : "Invert Camera: OFF";
        PlayerPrefs.SetInt("InvertCam", Services.PlayerController.isInvertedControls ? 1 : 0);
    }
}
