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
    Resume, Rewind, Settings, Eject, Back, InvertCamera, SetTime, ConfirmTime, CancelTime
}

public class VHSButtonsManager : MonoBehaviour
{
    private EventSystem EventSystem;
    private VHSButton[] vhsButtons;
    [SerializeField] private Canvas[] vhsButtonSets;
    public static Canvas canvas;

    private void Awake()
    {
        Services.VHSButtonsManager = this;
        EventSystem = EventSystem.current;
        canvas = GetComponent<Canvas>();
    }

    public VHSButton GetButtonByID(VHSButtons buttonId)
    {
        vhsButtons = FindObjectsOfType<VHSButton>();
        foreach (var button in vhsButtons)
        {
            if (button.buttonId == buttonId)
            {
                return button;
            }
        }
        return null;
    }

    /// <summary>
    /// Set button to select
    /// </summary>
    /// <param name="buttonIdToSelect">button to select</param>
    public void SetButtonSelected(VHSButtons buttonIdToSelect)
    {
        EventSystem.SetSelectedGameObject(GetButtonByID(buttonIdToSelect).gameObject);
    }
    
    /// <summary>
    /// Deselect all buttons
    /// </summary>
    public void DeselectAll()
    {
        EventSystem.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Toggle activation of a button
    /// </summary>
    /// <param name="buttonToDeactivate">button to activate / deactivate</param>
    /// <param name="isActivated"></param>
    public void SetButtonActivate(VHSButtons buttonToDeactivate, bool isActivated)
    {
        GetButtonByID(buttonToDeactivate).button.enabled = isActivated;
    }

    /// <summary>
    /// Switch to a different set of buttons
    /// </summary>
    /// <param name="set"></param>
    public void SwitchButtonSet(string set)
    {
        // reset all buttons
        foreach (var buttonSet in vhsButtonSets)
        {
            buttonSet.enabled = false;
            foreach (RectTransform child in buttonSet.GetComponent<RectTransform>())
                child.gameObject.SetActive(false); // turn off all objects
        }
        // switch to new button set
        var buttonSetId = set switch
        {
            "Menu" => 0,
            "Settings" => 1,
            "Rewind" => 2,
        };
        vhsButtonSets[buttonSetId].enabled = true; // turn on set canvas
        foreach (RectTransform child in vhsButtonSets[buttonSetId].GetComponent<RectTransform>())
        {
            if (child.gameObject.TryGetComponent(typeof(VHSButton), out var btn))
            {
                child.gameObject.SetActive(true); // turn on button objects
            }
        }
    }
}
