using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class RewindSet : MonoBehaviour
{
    // [SerializeField] private InputAction setTimeInputAction;
    [SerializeField] private GameObject timeSetHolder;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI leftArrowText;
    [SerializeField] private TextMeshProUGUI rightArrowText;
    private bool isSettingTime;
    // time
    private int setTime;
    private int rewindTime;
    // services
    private VHSDisplay VhsDisplay;
    private VHSButtonsManager VhsButtonsManager;
    private RewindManager RewindManager;

    void AssignServices()
    {
        VhsDisplay = Services.VHSDisplay;
        VhsButtonsManager = Services.VHSButtonsManager;
        RewindManager = Services.RewindManager;
    }

    void Awake()
    {
        AssignServices();
        timeSetHolder.SetActive(false);
    }

    void Update()
    {
        // keyboard input for adding / decreasing time
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame || Gamepad.current[GamepadButton.DpadLeft].wasPressedThisFrame)
        {
            DecreaseTime();
        }
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame || Gamepad.current[GamepadButton.DpadRight].wasPressedThisFrame)
        {
            IncreaseTime();
        }
        // during setting time
        if (!isSettingTime) return;
        rewindTime = Mathf.Clamp(rewindTime, 0, RewindManager.maxRewindTime);
        // do not allow confirm time if no rewind time is set
        VhsButtonsManager.SetButtonActivate(VHSButtons.ConfirmTime, rewindTime != 0);
    }

    // button functions
    public void BackFromRewind()
    {
        VhsDisplay.DisplayStatus(VHSStatuses.Paused);
        VhsButtonsManager.SwitchButtonSet("Menu");
        VhsButtonsManager.SetButtonSelected(VHSButtons.Rewind);
    }

    public void BeginSetTime()
    {
        isSettingTime = true;
        timeSetHolder.SetActive(true);
        // default rewind time to 0
        rewindTime = 0;
        // default set time to current survived time
        setTime = VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        timerText.text = VhsDisplay.GetFormattedTime(setTime);
        timerText.color = Color.white;
        rightArrowText.text = "";
        leftArrowText.text = "<";
        // de-activate other buttons
        VhsButtonsManager.SetButtonActivate(VHSButtons.Back, false);
        VhsButtonsManager.SetButtonActivate(VHSButtons.SetTime, false);
        // select cancel button by default
        VhsButtonsManager.SetButtonSelected(VHSButtons.CancelTime);
    }

    public void DecreaseTime()
    {
        // set rewind time (logic)
        rewindTime++;
        // set time on UI (visuals)
        setTime--;
        // clamp set time to not exceed max allowed rewind time
        if (setTime <= VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime) - RewindManager.maxRewindTime)
        {
            setTime = VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime) - RewindManager.maxRewindTime;
            timerText.color = Color.red;
            leftArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        // clamp set time to not go under started at time
        if (setTime <= Services.TimedGameMode.startAtSecond)
        {
            setTime = VhsDisplay.GetFormattedSecond(Services.TimedGameMode.startAtSecond);
            rewindTime = VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime); // only rewind to 0
            timerText.color = Color.red;
            leftArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        rightArrowText.text = ">";
        timerText.text = VhsDisplay.GetFormattedTime(setTime);
    }

    public void IncreaseTime()
    {
        // set rewind time (logic)
        rewindTime--;
        if (rewindTime == 0)
            VhsButtonsManager.SetButtonSelected(VHSButtons.CancelTime);
        // set time on UI (visuals)
        setTime++;
        if (setTime >= VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime))
        {
            setTime = VhsDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
            rightArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        timerText.color = Color.white;
        leftArrowText.text = "<";
        timerText.text = VhsDisplay.GetFormattedTime(setTime);
    }

    public void CancelSetTime()
    {
        isSettingTime = false;
        timeSetHolder.SetActive(false);
        // re-activate other buttons
        VhsButtonsManager.SetButtonActivate(VHSButtons.Back, true);
        VhsButtonsManager.SetButtonActivate(VHSButtons.SetTime, true);
        VhsButtonsManager.SetButtonSelected(VHSButtons.SetTime);
    }

    public void ConfirmSetTime()
    {
        CancelSetTime();
        // start rewind
        VhsDisplay.DisplayStatus(VHSStatuses.Rewind);
        RewindManager.rewindTime = rewindTime;
        StartCoroutine(RewindManager.RewindPosition(rewindTime));
    }
}
    