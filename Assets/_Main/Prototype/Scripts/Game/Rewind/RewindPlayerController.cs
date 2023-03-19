using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindPlayerController : MonoBehaviour
{
    [Range(3, 10)] public int maxRewindTime;
    public static bool isRewinding;
    public const int RewindSpeed = 3;
    // positions
    private CharacterController _characterController;
    private List<Vector3> playerPositions;
    private int lastSecond;
    private Vector3 rewindDir;

    void Awake()
    {
        Services.RewindPlayerController = this;
        _characterController = GetComponent<CharacterController>();
        playerPositions = new List<Vector3>(maxRewindTime);
        lastSecond = -1;
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

    public IEnumerator RewindPosition(int rewindSeconds)
    {
        isRewinding = true;
        // restart time scale for move
        Time.timeScale = 1;
        // turn off menu
        VHSButtonsManager.canvas.enabled = false;
        // TODO: begin rewind effect
        
        // check positions count
        int startCount = playerPositions.Count;
        for (int i = 1; i <= rewindSeconds; i++)
        {
            var timer = 0f;
            while (timer < 1f / RewindSpeed)
            {
                // TODO: clamp minimum rewind move distance to reduce jitter
                
                rewindDir = playerPositions[startCount - i] - transform.position;
                rewindDir = rewindDir.normalized;
                timer += Time.deltaTime;
                yield return null;
            }
            playerPositions.RemoveAt(startCount - i);
        }
        // set new position log time
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        Services.TimedGameMode.TogglePause(); // unpause game
        // TODO: stop rewind effect
        
        // end rewind
        isRewinding = false;
    }
}
