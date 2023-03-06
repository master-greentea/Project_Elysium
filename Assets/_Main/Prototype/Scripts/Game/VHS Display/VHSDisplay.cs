using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum VHSStatuses
{
    Play, Paused, Rewind
}

public class VHSDisplay : MonoBehaviour
{
    public static VHSDisplay Instance {get; private set;}
    private Animator _animator;
    [Header("VHS Texts")]
    [SerializeField] private TextMeshProUGUI timeDisplayText;
    [SerializeField] private TextMeshProUGUI statusDisplayText;

    void Awake()
    {
        Instance = this;
        _animator = GetComponent<Animator>();
    }

    public void DisplayTime(float timeToDisplay)
    {
        var minutes = Mathf.Floor(timeToDisplay / 60f);
        var seconds = Mathf.RoundToInt(timeToDisplay % 60f);
        var formattedMinutes = minutes.ToString();
        var formattedSeconds = seconds.ToString();
        
        // format with 0 in front of single digit
        if (minutes < 10) formattedMinutes += "0" + minutes;
        if (seconds < 10) formattedSeconds += "0" + Mathf.RoundToInt(seconds);

        timeDisplayText.text = formattedMinutes + ":" + formattedSeconds + " AM";
    }

    public void DisplayTime(float timeToDisplay, string prefix)
    {
        var minutes = Mathf.Floor(timeToDisplay / 60f);
        var seconds = Mathf.RoundToInt(timeToDisplay % 60f);
        var formattedMinutes = minutes.ToString();
        var formattedSeconds = seconds.ToString();
        
        // format with 0 in front of single digit
        if (minutes < 10) formattedMinutes = "0" + minutes;
        if (seconds < 10) formattedSeconds = "0" + Mathf.RoundToInt(seconds);

        timeDisplayText.text = prefix + " " + formattedMinutes + ":" + formattedSeconds;
    }

    public void DisplayStatus(VHSStatuses status)
    {
        // switch text
        statusDisplayText.text = status switch
        {
            VHSStatuses.Play => "> PLAY",
            VHSStatuses.Paused => "// PAUSED",
            VHSStatuses.Rewind => "<< REWIND"
        };
        // change text animation
        switch (status)
        {
            case VHSStatuses.Play:
                _animator.Play("VHS_Status_Play");
                break;
            case VHSStatuses.Paused:
                _animator.Play("VHS_Status_Paused");
                break;
        }
    }
}
