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

    public void TogglePause()
    {
        if (isGameEnded) return;
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        // pause menu set up
        Services.VHSDisplay.DisplayStatus(isGamePaused? VHSStatuses.Paused : VHSStatuses.Play);
        VHSButtonsManager.canvas.enabled = isGamePaused;
        Services.VHSButtonsManager.SetSelected(VHSButtons.Resume);
        TogglePlayerInput(Services.PlayerController.input.Player, false);
        // deselect all buttons if un-pausing
        if (isGamePaused) return;
        Services.VHSButtonsManager.DeselectAll();
        TogglePlayerInput(Services.PlayerController.input.Player, true);
    }
    
    private void TogglePlayerInput(PrototypePlayerInput.PlayerActions actions, bool isActive)
    {
        if (isActive) actions.Enable();
        else actions.Disable();
    }

    public virtual void EndGame()
    {
        
    }
}
