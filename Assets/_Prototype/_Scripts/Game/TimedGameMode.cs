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
    }

    public static void AddTime(float addTimeAmount)
    {
        SurvivedTime += addTimeAmount;
    }

    public override void TogglePause()
    {
        base.TogglePause();
        // only allow rewind when more than 10 seconds has passed
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Rewind, RewindManager.CanRewind);
    }
}
