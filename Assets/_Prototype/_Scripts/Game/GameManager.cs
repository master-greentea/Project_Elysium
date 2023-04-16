using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGamePaused { get; private set; }
    public static bool isConsole { get; private set; }
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
        PauseManager.canvas.enabled = isGamePaused;
        Services.PauseManager.SwitchButtonSet("Menu");
        // default to select resume
        Services.PauseManager.SetButtonSelected(VHSButtons.Resume);
        TogglePlayerInput(Services.PlayerController.input, false);
        // deselect all buttons if un-pausing
        if (isGamePaused) return;
        Services.PauseManager.DeselectAll();
        TogglePlayerInput(Services.PlayerController.input, true);
    }

    public virtual void ToggleConsole()
    {
        if (isGameEnded) return;
        if (isGamePaused) TogglePause();
        isConsole = !isConsole;
        Time.timeScale = isConsole ? 0f : 1f;
        // console ui set up
        Services.VHSDisplay.DisplayStatus(isConsole? VHSStatuses.Console : VHSStatuses.Play);
        ConsoleManager.canvas.enabled = isConsole;
        // turn on input field for console
        Services.ConsoleManager.consoleInput.interactable = isConsole;
        // default to select resume
        Services.PauseManager.SetButtonSelected(VHSButtons.ConsoleResume);
        TogglePlayerInput(Services.PlayerController.input, false);
        if (isConsole) return;
        Services.PauseManager.DeselectAll();
        TogglePlayerInput(Services.PlayerController.input, true);
    }

    public virtual void EndGame()
    {
        isGameEnded = true;
        Time.timeScale = 0;
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Error);
        // turn on menu
        PauseManager.canvas.enabled = isGameEnded;
        Services.PauseManager.SwitchButtonSet("Menu");
        // default to select eject
        Services.PauseManager.SetButtonSelected(VHSButtons.Eject);
        // deactivate resume & rewind
        Services.PauseManager.SetButtonActivate(VHSButtons.Resume, false);
        Services.PauseManager.SetButtonActivate(VHSButtons.Rewind, false);
    }
}
