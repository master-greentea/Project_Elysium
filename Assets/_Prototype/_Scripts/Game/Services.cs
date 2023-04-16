using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;

public static class Services
{
    // Player
    public static PlayerController PlayerController;
    public static RewindManager RewindManager;
    public static SkipManager SkipManager;
    // AI
    public static EnemyAgent EnemyAgent;
    // Managers
    public static GameManager GameManager;
    public static ConsoleManager ConsoleManager;
    public static VHSDisplay VHSDisplay;
    public static PauseManager PauseManager;
    // Game modes
    public static TimedGameMode TimedGameMode;
}
