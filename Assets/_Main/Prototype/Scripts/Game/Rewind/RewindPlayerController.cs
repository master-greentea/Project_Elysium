using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindPlayerController : MonoBehaviour
{
    [Range(3, 10)] public int maxRewindTime;
    public static bool isRewinding;
    public static bool canRewind { get; private set; }
    public const int RewindSpeed = 2;
    // positions
    private CharacterController _characterController;
    private List<Vector3> playerPositions;
    private int lastSecond;
    private Vector3 rewindDir;
    public static bool isRewindMoving;
    // camera input
    private class LoggedCamera
    {
        public readonly int timeStampInSeconds;
        public readonly camDirection loggedDirection;
        public LoggedCamera(camDirection direction)
        {
            timeStampInSeconds = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
            loggedDirection = direction;
        }
    }
    private List<LoggedCamera> cameraPositions;

    void Awake()
    {
        Services.RewindPlayerController = this;
        _characterController = GetComponent<CharacterController>();
        playerPositions = new List<Vector3>(maxRewindTime);
        cameraPositions = new List<LoggedCamera>();
        lastSecond = -1;
        StartCoroutine(RewindCooldown());
    }

    IEnumerator RewindCooldown()
    {
        yield return new WaitForSeconds(maxRewindTime * 2);
        // TODO: rewind ready notification
        Services.VHSDisplay.DisplayNotification("Rewind: Ready");
        canRewind = true;
    }
    
    void Update()
    {
        LogPosition();
    }

    void FixedUpdate()
    {
        if (!isRewinding) return;
        // rewind move
        _characterController.Move(rewindDir * Time.deltaTime * Services.PlayerController.walkSpeed * RewindSpeed);
    }

    void LogPosition()
    {
        // do not log position if is rewinding
        if (isRewinding) return;
        // check if time has passed
        int thisSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        if (thisSecond <= lastSecond) return;
        // log position
        if (playerPositions.Count < playerPositions.Capacity)
        {
            playerPositions.Add(transform.position);
        }
        else
        {
            playerPositions.RemoveAt(0);
            playerPositions.Add(transform.position);
        }
        // set new position log time
        lastSecond = thisSecond;
    }
    
    public void LogCamera(camDirection direction)
    {
        // do not log position if is rewinding
        if (isRewinding) return;
        // create camera log
        LoggedCamera log = new LoggedCamera(direction);
        // check if there's already a log at the same second
        foreach (var camLog in cameraPositions)
        {
            if (camLog.timeStampInSeconds != log.timeStampInSeconds) continue;
            // only keep the latest one if triggered multiple logs in one second
            cameraPositions.Remove(camLog);
            break;
        }
        cameraPositions.Add(log);
    }

    void CheckCameraPositionLog()
    {
        foreach (var camLog in cameraPositions)
        {
            if (camLog.timeStampInSeconds == Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime))
            {
                Services.PlayerController.SwitchCamera(camLog.loggedDirection, .3f);
            }
        }
    }

    public IEnumerator RewindPosition(int rewindSeconds)
    {
        isRewinding = true;
        // restart time scale for move
        Time.timeScale = 1;
        // turn off menu
        VHSButtonsManager.canvas.enabled = false;
        Services.VHSDisplay.DisplayNotification("");
        // TODO: begin rewind effect
        
        // check positions count
        int startCount = playerPositions.Count;
        for (int i = 1; i <= rewindSeconds; i++)
        {
            // each second in rewind
            CheckCameraPositionLog(); // play logged camera movement
            var timer = 0f;
            while (timer < 1f / RewindSpeed)
            {
                rewindDir = playerPositions[startCount - i] - transform.position;
                isRewindMoving = rewindDir.magnitude > .5f; // check magnitude to determine whether moving or not
                rewindDir = rewindDir.normalized; // get normalized vector for move
                timer += Time.deltaTime;
                yield return null;
            }
            playerPositions.RemoveAt(startCount - i);
            // TODO: remove used camera log
            
        }
        isRewindMoving = false;
        // set new position log time
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        // TODO: clean up camera log list
        
        Services.TimedGameMode.TogglePause(); // unpause game
        // TODO: stop rewind effect
        
        // end rewind
        isRewinding = false;
        canRewind = false;
        StartCoroutine(RewindCooldown());
    }
}
