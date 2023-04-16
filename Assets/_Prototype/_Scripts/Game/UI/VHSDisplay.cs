using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum VHSStatuses
{
    Play, Paused, Error, Settings, Rewind, Console
}

public class VHSDisplay : MonoBehaviour
{
    private Animator _animator;
    [Header("VHS Texts")]
    [SerializeField] private TextMeshProUGUI timeDisplayText;
    [SerializeField] private TextMeshProUGUI statusDisplayText;
    [SerializeField] private TextMeshProUGUI notificationText;

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
        var formattedMinutes = seconds == 60 ? (minutes++).ToString() : minutes.ToString();
        var formattedSeconds = seconds == 60 ? "00" : seconds.ToString();
        
        // format with 0 in front of single digit
        if (minutes < 10) formattedMinutes = "0" + minutes;
        if (seconds < 10) formattedSeconds = "0" + Mathf.RoundToInt(seconds);
        
        return formattedMinutes + ":" + formattedSeconds;
    }

    /// <summary>
    /// Get formatted time in seconds
    /// </summary>
    /// <param name="time">time to format</param>
    /// <returns>Formatted time in seconds</returns>
    public int GetFormattedSecond(float time)
    {
        var minutes = Mathf.Floor(time / 60f);
        return Mathf.RoundToInt(time % 60f) + (int)minutes * 60;
    }

    /// <summary>
    /// Change the status display to a new status
    /// </summary>
    /// <param name="status">status to change to</param>
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
            VHSStatuses.Console => ".CONSOLE",
        };
        // change text animation
        switch (status)
        {
            case VHSStatuses.Play:
            case VHSStatuses.Console:
                _animator.Play("VHS_Status_Play");
                break;
            case VHSStatuses.Paused:
            case VHSStatuses.Settings:
            case VHSStatuses.Rewind:
            case VHSStatuses.Error:
                _animator.Play("VHS_Status_Paused");
                break;
        }
    }

    /// <summary>
    /// Display notification on the bottom right
    /// </summary>
    /// <param name="notification"></param>
    public void DisplayNotification(string notification)
    {
        // TODO: add notification as a list
        
        notificationText.text = notification;
    }
}
