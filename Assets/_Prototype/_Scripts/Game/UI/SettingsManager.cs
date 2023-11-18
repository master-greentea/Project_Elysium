using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private VHSButton invertCameraButton;
    [SerializeField] private TMP_InputField apiInputField;

    private void Update()
    {
        MatchSettingButtonTexts();
    }

    private void MatchSettingButtonTexts()
    {
        invertCameraButton.buttonText = playerController.isInvertedControls ? "Invert Camera: ON" : "Invert Camera: OFF";
    }

    public void InvertCamera()
    {
        playerController.isInvertedControls = !playerController.isInvertedControls;
        PlayerPrefs.SetInt("InvertCam", playerController.isInvertedControls ? 1 : 0);
    }
    
    public void APIEdit()
    {
        apiInputField.interactable = true;
        apiInputField.readOnly = false;
        apiInputField.ActivateInputField();
        apiInputField.Select();
        apiInputField.textComponent.color = Color.white;
    }

    public void APIConfirm()
    {
        apiInputField.DeactivateInputField();
        apiInputField.interactable = false;
        apiInputField.readOnly = true;
        apiInputField.textComponent.color = Color.gray;
    }
}
