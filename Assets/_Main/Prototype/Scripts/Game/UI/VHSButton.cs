using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI tmp;
    [HideInInspector] public Button button;
    [HideInInspector] public bool isMouseOver;

    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        isMouseOver = true;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        isMouseOver = false;
    }

    public void SetText(string text)
    {
        tmp.text = text;
    }
    
    // menu functions
    public void Resume()
    {
        Services.GameManager.TogglePause();
    }

    public void Rewind()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Settings()
    {
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Settings);
        Services.VHSButtonsManager.SwitchButtonSet("Settings");
        Services.VHSButtonsManager.SetSelected(VHSButtons.Back);
    }

    public void Eject()
    {
        Application.Quit();
    }
    
    // settings functions
    public void BackToMenu()
    {
        Services.VHSDisplay.DisplayStatus(VHSStatuses.Paused);
        Services.VHSButtonsManager.SwitchButtonSet("Menu");
        Services.VHSButtonsManager.SetSelected(VHSButtons.Settings);
    }
}
