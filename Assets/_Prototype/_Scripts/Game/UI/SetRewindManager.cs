using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetRewindManager : MonoBehaviour
{
    // [SerializeField] private InputAction setTimeInputAction;
    [SerializeField] private VHSButton confirmButton;
    [SerializeField] private VHSButton cancelButton;
    [SerializeField] private GameObject timeSetHolder;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI leftArrowText;
    [SerializeField] private TextMeshProUGUI rightArrowText;
    private bool isSettingTime;
    private int setTime;
    private int rewindTime;
    // services
    private VHSDisplay vhsDisplay;
    private RewindManager rewindManager;

    void AssignServices()
    {
        vhsDisplay = Services.VHSDisplay;
        rewindManager = Services.RewindManager;
    }

    void Awake()
    {
        AssignServices();
    }

    void Update()
    {
        isSettingTime = timeSetHolder.activeSelf;
        // during setting time
        if (!isSettingTime) return;
        // keyboard input for adding / decreasing time
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
        {
            DecreaseTime();
        }
        if (Keyboard.current[Key.RightArrow].wasPressedThisFrame)
        {
            IncreaseTime();
        }
        
        rewindTime = Mathf.Clamp(rewindTime, 0, rewindManager.maxRewindTimeAmount);
        // do not allow confirm time if no rewind time is set
        confirmButton.button.enabled = rewindTime != 0;
    }

    public void BeginSetTime()
    {
        // default rewind time to 0
        rewindTime = 0;
        // default set time to current survived time
        setTime = vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
        timerText.text = vhsDisplay.GetFormattedTime(setTime);
        timerText.color = Color.white;
        rightArrowText.text = "";
        leftArrowText.text = "<";
    }

    public void DecreaseTime()
    {
        // set rewind time (logic)
        rewindTime++;
        // set time on UI (visuals)
        setTime--;
        // clamp set time to not exceed max allowed rewind time
        if (setTime <= vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime) - rewindManager.maxRewindTimeAmount)
        {
            setTime = vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime) - rewindManager.maxRewindTimeAmount;
            timerText.color = Color.red;
            leftArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        // clamp set time to not go under started at time
        if (setTime <= Services.TimedGameMode.startAtSecond + 1)
        {
            setTime = vhsDisplay.GetFormattedSecond(Services.TimedGameMode.startAtSecond + 1);
            rewindTime = vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime); // only rewind to 0
            timerText.color = Color.red;
            leftArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        rightArrowText.text = ">";
        timerText.text = vhsDisplay.GetFormattedTime(setTime);
    }

    public void IncreaseTime()
    {
        // set rewind time (logic)
        rewindTime--;
        if (rewindTime == 0)
        {
            cancelButton.button.Select();
        }
        // set time on UI (visuals)
        setTime++;
        if (setTime >= vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime))
        {
            setTime = vhsDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
            rightArrowText.text = "";
            timerText.GetComponent<Animator>().Play("SetTimeShake");
        }
        timerText.color = Color.white;
        leftArrowText.text = "<";
        timerText.text = vhsDisplay.GetFormattedTime(setTime);
    }

    public void BeginRewind()
    {
        // start rewind
        RewindManager.SetRewindTime = rewindTime;
        StartCoroutine(rewindManager.Rewind(rewindTime));
    }
}
    