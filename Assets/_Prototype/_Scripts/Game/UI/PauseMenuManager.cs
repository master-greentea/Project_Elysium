using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static Canvas pauseCanvas;
    [SerializeField] private VHSButton firstSelectButton;
    [SerializeField] private VHSButton endFirstSelectButton;

    [SerializeField] private VHSButton[] endDisableButton;
    
    private void Awake()
    {
        Services.PauseMenuManager = this;
        pauseCanvas = GetComponent<Canvas>();
    }

    public void ToggleEnd()
    {
        pauseCanvas.enabled = true;
        foreach (var button in endDisableButton)
        {
            button.button.enabled = false;
        }
        endFirstSelectButton.button.Select();
    }

    public void TogglePause()
    {
        GameManager.PauseGame(true);
        Services.VHSDisplay.DisplayStatus(GameManager.IsGamePaused? 1 : 0);
        pauseCanvas.enabled = GameManager.IsGamePaused;
        firstSelectButton.button.Select();
    }

    public void Eject()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Application.Quit();
    }

}
