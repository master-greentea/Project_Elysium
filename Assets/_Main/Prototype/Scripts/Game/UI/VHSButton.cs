using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler
{
    private EventSystem EventSystem;
    public TextMeshProUGUI tmp;
    private string buttonText;
    public VHSButtons buttonId;
    [HideInInspector] public Button button;
    // services
    private GameManager GameManager;
    private PlayerController PlayerController;
    private VHSDisplay VhsDisplay;
    private VHSButtonsManager VhsButtonsManager;

    void AssignServices()
    {
        VhsDisplay = Services.VHSDisplay;
        VhsButtonsManager = Services.VHSButtonsManager;
        GameManager = Services.GameManager;
        PlayerController = Services.PlayerController;
    }

    void Awake()
    {
        AssignServices();
        button = GetComponent<Button>();
        EventSystem = EventSystem.current;
        buttonText = buttonId switch
        {
            VHSButtons.Resume => "Resume",
            VHSButtons.Rewind => "Rewind",
            VHSButtons.Settings => "Settings",
            VHSButtons.Eject => "Eject",
            VHSButtons.Back => "Back",
            VHSButtons.InvertCamera => PlayerController.isInvertedControls ? "Invert Camera: ON" : "Invert Camera: OFF",
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
        if (button.enabled) VhsButtonsManager.SetButtonSelected(buttonId);
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
        GameManager.TogglePause();
    }

    public void Rewind()
    {
        VhsButtonsManager.SwitchButtonSet("Rewind");
        VhsButtonsManager.SetButtonSelected(VHSButtons.Back);
    }

    public void Settings()
    {
        VhsDisplay.DisplayStatus(VHSStatuses.Settings);
        VhsButtonsManager.SwitchButtonSet("Settings");
        VhsButtonsManager.SetButtonSelected(VHSButtons.Back);
    }

    public void Eject()
    {
        Application.Quit();
    }
    
    // settings functions
    public void BackFromSettings()
    {
        VhsDisplay.DisplayStatus(VHSStatuses.Paused);
        VhsButtonsManager.SwitchButtonSet("Menu");
        VhsButtonsManager.SetButtonSelected(VHSButtons.Settings);
    }

    public void InvertCamera()
    {
        PlayerController.isInvertedControls = !PlayerController.isInvertedControls;
        buttonText = PlayerController.isInvertedControls ? "Invert Camera: ON" : "Invert Camera: OFF";
        PlayerPrefs.SetInt("InvertCam", PlayerController.isInvertedControls ? 1 : 0);
    }
}
