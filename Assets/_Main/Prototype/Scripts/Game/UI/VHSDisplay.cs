using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum VHSStatuses
{
    Play, Paused, Error, Settings, Rewind
}

public class VHSDisplay : MonoBehaviour
{
    private Animator _animator;
    [Header("VHS Texts")]
    [SerializeField] private TextMeshProUGUI timeDisplayText;
    [SerializeField] private TextMeshProUGUI statusDisplayText;

    void Awake()
    {
        Services.VHSDisplay = this;
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Display current play time in default format
    /// </summary>
    /// <param name="timeToDisplay"></param>
    public void DisplayTime(float timeToDisplay)
    {
        timeDisplayText.text = GetFormattedTime(timeToDisplay) + " AM";
    }

    /// <summary>
    /// Display current play time with a prefix
    /// </summary>
    /// <param name="timeToDisplay"></param>
    /// <param name="prefix"></param>
    public void DisplayTime(float timeToDisplay, string prefix)
    {
        timeDisplayText.text = prefix + " " + GetFormattedTime(timeToDisplay);
    }

    /// <summary>
    /// Get formatted time in string
    /// </summary>
    /// <param name="time">time to format</param>
    /// <returns>Formatted time string</returns>
    public string GetFormattedTime(float time)
    {
        var minutes = Mathf.Floor(time / 60f);
        var seconds = Mathf.RoundToInt(time % 60f);
        var formattedMinutes = minutes.ToString();
        var formattedSeconds = seconds.ToString();
        
        // format with 0 in front of single digit
        if (minutes < 10) formattedMinutes = "0" + minutes;
        if (seconds < 10) formattedSeconds = "0" + Mathf.RoundToInt(seconds);
        
        return formattedMinutes + ":" + formattedSeconds;
    }

    public int GetFormattedSecond(float time)
    {
        return Mathf.RoundToInt(time % 60f);
    }

    public void DisplayStatus(VHSStatuses status)
    {
        // switch text
        statusDisplayText.text = status switch
        {
            VHSStatuses.Play => "> PLAY",
            VHSStatuses.Paused => "// PAUSED",
            VHSStatuses.Error => "! ERROR",
            VHSStatuses.Settings => "// SETTINGS",
            VHSStatuses.Rewind => "<< REWIND",
        };
        // change text animation
        switch (status)
        {
            case VHSStatuses.Play:
                _animator.Play("VHS_Status_Play");
                break;
            case VHSStatuses.Paused:
            case VHSStatuses.Settings:
            case VHSStatuses.Rewind:
                _animator.Play("VHS_Status_Paused");
                break;
        }
    }
}
