using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGameMode : GameManager
{
    public float survivedTime {get; private set;}

    void Awake()
    {
        Services.TimedGameMode = this;
        survivedTime = 0;
    }
    
    void Update()
    {
        survivedTime += Time.deltaTime;
        Services.VHSDisplay.DisplayTime(survivedTime, "ST");
    }

    public override void TogglePause()
    {
        base.TogglePause();
        // only allow rewind when more than 10 seconds has passed
        Services.VHSButtonsManager.SetButtonActivate(VHSButtons.Rewind, Services.TimedGameMode.survivedTime > 10);
    }
}
