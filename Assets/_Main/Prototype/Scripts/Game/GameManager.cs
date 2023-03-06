using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public static bool gamePaused { get; private set; }
    private PrototypePlayerInput input;

    void Awake()
    {
        Instance = this;
        input = new PrototypePlayerInput();
        input.UIControls.Enable();
        input.UIControls.Pause.performed += TogglePause;
    }

    void TogglePause(InputAction.CallbackContext ctx)
    {
        gamePaused = !gamePaused;
        VHSDisplay.Instance.DisplayStatus(gamePaused? VHSStatuses.Paused : VHSStatuses.Play);
        Time.timeScale = gamePaused ? 0f : 1f;
    }
}
