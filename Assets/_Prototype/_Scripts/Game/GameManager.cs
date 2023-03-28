using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGamePaused { get; private set; }
    public static bool isGameEnded { get; protected set; }

    void Awake()
    {
        Services.GameManager = this;
        Time.timeScale = 1;
        isGameEnded = false;
        isGamePaused = false;
    }

    private void TogglePlayerInput(PrototypePlayerInput actions, bool isActive)
    {
        if (isActive) actions.Player.Enable();
        else actions.Player.Disable();
    }

    public virtual void TogglePause()
    {
        if (isGameEnded) return;
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        // menu setup
        Services.VHSDisplay.DisplayStatus(isGamePaused? VHSStatuses.Paused : VHSStatuses.Play);
        // turn on menu
        VHSButtonsManager.canvas.enabled = isGamePaused;
        Services.VHSButtonsManager.SwitchButtonSet("Menu");
        // default to select resume
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Resume);
        TogglePlayerInput(Services.PlayerController.input, false);
        // deselect all buttons if un-pausing
        if (isGamePaused) return;
        Services.VHSButtonsManager.DeselectAll();
        TogglePlayerInput(Services.PlayerController.input, true);
    }

    public virtual void EndGame()
    {
        isGameEnded = true;
        Time.timeScale = 0;
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Error);
        // turn on menu
        VHSButtonsManager.canvas.enabled = isGameEnded;
        Services.VHSButtonsManager.SwitchButtonSet("Menu");
        // default to select eject
        Services.VHSButtonsManager.SetButtonSelected(VHSButtons.Eject);
        // deactivate resume & rewind
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Resume, false);
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Rewind, false);
    }
}
