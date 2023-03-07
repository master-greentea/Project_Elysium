using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGameMode : GameManager
{
    public static float survivedTime {get; private set;}

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

    public override void EndGame()
    {
        Time.timeScale = 0;
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Error);
        VHSButtonsManager.canvas.enabled = true;
        Services.VHSButtonsManager.SetSelected(VHSButtons.Rewind);
        Services.VHSButtonsManager.DeactivateButton(VHSButtons.Resume);
        isGameEnded = true;
    }
}
