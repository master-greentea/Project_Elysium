using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static bool IsGamePaused { get; set; }
    public static bool IsGameEnded { get; protected set; }

    void Awake()
    {
        Services.GameManager = this;
        Time.timeScale = 1;
        GameStart();
    }

    public static void GameStart()
    {
        IsGameEnded = false;
        IsGamePaused = false;
    }
    
    public static void EndGame()
    {
        IsGameEnded = true;
        Time.timeScale = 0;
        Services.VHSDisplay.DisplayStatus(2);
        Services.PauseMenuManager.ToggleEnd();
    }

    public static void PauseGame(bool disableControls)
    {
        if (IsGameEnded) return;
        IsGamePaused = !IsGamePaused;
        Time.timeScale = IsGamePaused ? 0f : 1f;
        if (disableControls)
        {
            Services.PlayerController.TogglePlayerInput(!IsGamePaused);
            if (!IsGamePaused) EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
