using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public static bool gamePaused { get; protected set; }
    private PrototypePlayerInput input;
    
    public static bool isDead;
    
    void Awake()
    {
        Instance = this;
        input = new PrototypePlayerInput();
        input.UI.Enable();
        input.UI.Pause.performed += TogglePause;
    }

    private static void TogglePause(InputAction.CallbackContext ctx)
    {
        gamePaused = !gamePaused;
        VHSDisplay.Instance.DisplayStatus(gamePaused? VHSStatuses.Paused : VHSStatuses.Play);
        Time.timeScale = gamePaused ? 0f : 1f;
    }

    public virtual void EndGame()
    {
        
    }
}
