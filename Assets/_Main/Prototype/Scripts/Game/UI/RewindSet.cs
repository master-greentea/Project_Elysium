using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RewindSet : MonoBehaviour
{
    [SerializeField] private GameObject timeSetHolder;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject confirmObject;
    private bool isSettingTime;
    // time
    private float currentTime;

    void Awake()
    {
        timeSetHolder.SetActive(false);
    }

    void Update()
    {
        timeSetHolder.SetActive(isSettingTime);
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
        {
            DecreaseTime();
        }
        if (Keyboard.current[Key.RightArrow].wasPressedThisFrame)
        {
            IncreaseTime();
        }
    }

    // button functions
    public void BackFromRewind()
    {
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Paused);
        Services.VHSButtonsManager.SwitchButtonSet("Menu");
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Rewind);
    }

    public void BeginSetTime()
    {
        Services.VHSButtonsManager.DeselectAll();
        isSettingTime = true;
        currentTime = Services.TimedGameMode.survivedTime;
        timerText.text = Services.VHSDisplay.GetFormattedTime(currentTime);
        // de-activate other buttons
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Back, false);
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.SetTime, false);
        
        EventSystem e = EventSystem.current;
        e.SetSelectedGameObject(confirmObject);
    }

    public void DecreaseTime()
    {
        currentTime--;
        if (currentTime <= Services.TimedGameMode.survivedTime - 10)
        {
            currentTime = Services.TimedGameMode.survivedTime - 10;
            timerText.color = Color.red;
        }
        else if (currentTime <= 0)
        {
            currentTime = 0;
        }
        else
        {
            timerText.color = Color.white;
        }
        timerText.text = Services.VHSDisplay.GetFormattedTime(currentTime);
    }

    public void IncreaseTime()
    {
        currentTime++;
        if (currentTime >= Services.TimedGameMode.survivedTime)
        {
            currentTime = Services.TimedGameMode.survivedTime;
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }
        timerText.text = Services.VHSDisplay.GetFormattedTime(currentTime);
    }

    public void CancelSetTime()
    {
        isSettingTime = false;
        // re-activate other buttons
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Back, true);
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.SetTime, true);
        
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.SetTime);
    }
}
    