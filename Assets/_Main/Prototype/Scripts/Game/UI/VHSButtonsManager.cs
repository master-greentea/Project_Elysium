using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum VHSButtons
{
    Resume, Rewind, Settings, Eject, Back,
}

public class VHSButtonsManager : MonoBehaviour
{
    private EventSystem EventSystem;
    [SerializeField] private VHSButton[] vhsButtons;
    [SerializeField] private string[] defaultTexts;
    public static Canvas canvas;

    private void Awake()
    {
        Services.VHSButtonsManager = this;
        EventSystem = EventSystem.current;
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        foreach (var vhsButton in vhsButtons.Select((val, i) => new {val, i}))
        {
            // set button text based on whether they are selected
            if (vhsButton.val.gameObject == EventSystem.currentSelectedGameObject)
            {
                vhsButton.val.SetText("> " + defaultTexts[vhsButton.i]);
            }
            else vhsButton.val.SetText(defaultTexts[vhsButton.i]);
        }
    }

    /// <summary>
    /// Set button to select
    /// </summary>
    /// <param name="buttonToSelect">button to select</param>
    public void SetSelected(VHSButtons buttonToSelect)
    {
        vhsButtons[(int)buttonToSelect].button.Select();
    }
    
    /// <summary>
    /// Deselect all buttons
    /// </summary>
    public void DeselectAll()
    {
        EventSystem.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Deactivate a button
    /// </summary>
    /// <param name="buttonToDeactivate">button to deactivate</param>
    public void DeactivateButton(VHSButtons buttonToDeactivate)
    {
        vhsButtons[(int)buttonToDeactivate].button.enabled = false;
        vhsButtons[(int)buttonToDeactivate].tmp.color = Color.black;
    }

    /// <summary>
    /// Switch to a different set of buttons, "Menu" or "Settings"
    /// </summary>
    /// <param name="set"></param>
    public void SwitchButtonSet(string set)
    {
        for (var i = 0; i < vhsButtons.Length; i++)
        {
            switch (set)
            {
                case "Menu":
                    vhsButtons[i].gameObject.SetActive(i < 4);
                    break;
                case "Settings":
                    vhsButtons[i].gameObject.SetActive(!(i < 4));
                    break;
            }
        }
    }
}
