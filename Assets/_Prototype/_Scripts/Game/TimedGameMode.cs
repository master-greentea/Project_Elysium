using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimedGameMode : GameManager
{
    public static float SurvivedTime {get; private set;}
    public float startAtSecond;

    void Awake()
    {
        Services.TimedGameMode = this;
        SurvivedTime = startAtSecond;
    }
    
    void Update()
    {
        if (!RewindManager.isRewinding) SurvivedTime += Time.deltaTime;
        Services.VHSDisplay.DisplayTime(SurvivedTime);
        // only allow rewind when more than 10 seconds has passed
        // Services.PauseManager.SetButtonActivate(VHSButtons.Rewind, RewindManager.CanRewind);
    }

    public static void AddTime(float addTimeAmount)
    {
        SurvivedTime += addTimeAmount;
    }
}
