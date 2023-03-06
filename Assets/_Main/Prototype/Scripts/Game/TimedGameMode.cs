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
    public static float survivedTime = 0f;

    void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        survivedTime += Time.deltaTime;
        VHSDisplay.Instance.DisplayTime(survivedTime, "ST");
    }
}
