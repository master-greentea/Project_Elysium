using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimedGameMode : GameManager
{
    public static float survivedTime {get; private set;}
    public float startAtSecond;

    void Awake()
    {
        Services.TimedGameMode = this;
        survivedTime = startAtSecond;
    }
    
    void Update()
    {
        if (RewindManager.isRewinding) survivedTime -= Time.deltaTime * RewindManager.RewindSpeed;
        else survivedTime += Time.deltaTime;
        Services.VHSDisplay.DisplayTime(survivedTime);
    }

    public static void TimeSkip(float skipTimeAmount)
    {
        survivedTime += skipTimeAmount;
    }

    public override void TogglePause()
    {
        base.TogglePause();
        // only allow rewind when more than 10 seconds has passed
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Rewind, RewindManager.canRewind);
    }
}
