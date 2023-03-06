using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimedGameMode : GameManager
{
    public static TimedGameMode Instance {get; private set;}

    [Header("Game Mode Settings")]
    [SerializeField] private Canvas deathCanvas;
    public static float survivedTime {get; private set;}

    void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        survivedTime += Time.deltaTime;
        VHSDisplay.Instance.DisplayTime(survivedTime, "ST");
        if (isDead) EndGame();
    }

    public override void EndGame()
    {
        deathCanvas.enabled = isDead;
        Time.timeScale = 0;
        gamePaused = true;
    }
}
