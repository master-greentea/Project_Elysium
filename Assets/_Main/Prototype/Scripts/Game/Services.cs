using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;

public static class Services
{
    // Player
    public static PlayerController PlayerController;
    public static RewindManager RewindManager;
    // AI
    public static EnemyAgent EnemyAgent;
    // Managers
    public static GameManager GameManager;
    public static VHSDisplay VHSDisplay;
    public static VHSButtonsManager VHSButtonsManager;
    // Game modes
    public static TimedGameMode TimedGameMode;
}
