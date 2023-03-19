using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RewindManager : MonoBehaviour
{
    [Range(3, 10)] public int maxRewindTime;
    [SerializeField] private ScriptableRendererFeature rewindBlit;
    public static bool isRewinding;
    public static bool canRewind { get; private set; }
    public static int rewindTime { get; set; }
    public const int RewindSpeed = 2;
    // positions
    private CharacterController _characterController;
    private static List<Vector3> playerPositions;
    private int lastSecond;
    private Vector3 rewindDir;
    public static bool isRewindMoving;
    // camera input
    private class CameraRewindInfo
    {
        public readonly int timeStampInSeconds;
        public readonly camDirection loggedDirection;
        public CameraRewindInfo(camDirection direction)
        {
            timeStampInSeconds = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
            loggedDirection = direction;
        }
    }
    private static List<CameraRewindInfo> CameraRewindInfoList;
    // enemy
    public class EnemyRewindInfo
    {
        public readonly Vector3 position;
        public readonly EnemyStateId stateId;
        public EnemyRewindInfo(Vector3 position, EnemyStateId stateId)
        {
            this.position = position;
            this.stateId = stateId;
        }
    }
    public static List<EnemyRewindInfo> EnemyRewindInfoList;

    void Awake()
    {
        Services.RewindManager = this;
        _characterController = GetComponent<CharacterController>();
        playerPositions = new List<Vector3>(maxRewindTime);
        CameraRewindInfoList = new List<CameraRewindInfo>();
        EnemyRewindInfoList = new List<EnemyRewindInfo>(maxRewindTime);
        lastSecond = -1;
        StartCoroutine(RewindCooldown());
    }

    IEnumerator RewindCooldown()
    {
        yield return new WaitForSeconds(maxRewindTime * 2);
        Services.VHSDisplay.DisplayNotification("Rewind: Ready");
        canRewind = true;
    }
    
    void Update()
    {
        LogInformationPerSecond();
    }

    void FixedUpdate()
    {
        if (!isRewinding) return;
        // rewind player move
        _characterController.Move(rewindDir * Time.deltaTime * Services.PlayerController.walkSpeed * RewindSpeed);
    }

    void LogInformationPerSecond()
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
            EnemyRewindInfoList.Add(new EnemyRewindInfo(Services.EnemyAgent.transform.position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        else
        {
            playerPositions.RemoveAt(0);
            EnemyRewindInfoList.RemoveAt(0);
            playerPositions.Add(transform.position);
            EnemyRewindInfoList.Add(new EnemyRewindInfo(Services.EnemyAgent.transform.position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        // set new position log time
        lastSecond = thisSecond;
        // clean up camera log list
        foreach (var (value, i) in CameraRewindInfoList.Select((value, i) => ( value, i )))
        {
            if (value.timeStampInSeconds < TimedGameMode.survivedTime - maxRewindTime) continue;
            CameraRewindInfoList.RemoveRange(0, i);
            break;
        }
    }
    
    public void LogCamera(camDirection direction)
    {
        // do not log position if is rewinding
        if (isRewinding) return;
        // create camera log
        CameraRewindInfo log = new CameraRewindInfo(direction);
        // check if there's already a log at the same second
        foreach (var camLog in CameraRewindInfoList)
        {
            if (camLog.timeStampInSeconds != log.timeStampInSeconds) continue;
            // only keep the latest one if triggered multiple logs in one second
            CameraRewindInfoList.Remove(camLog);
            break;
        }
        CameraRewindInfoList.Add(log);
    }

    void CheckCameraPositionLog()
    {
        foreach (var camLog in CameraRewindInfoList)
        {
            if (camLog.timeStampInSeconds == Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime))
            {
                Services.PlayerController.SwitchCamera(camLog.loggedDirection, .3f);
                CameraRewindInfoList.Remove(camLog);
                break;
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
        // begin rewind effect
        rewindBlit.SetActive(true);
        // begin enemy rewind
        Services.EnemyAgent.EnemyStateMachine.ChangeState(EnemyStateId.Rewind);
        // for each second in rewind
        for (int i = 1; i <= rewindSeconds; i++)
        {
            // play logged camera movement
            CheckCameraPositionLog();
            // set enemy rewind to last position
            RewindState.positionToRewindTo = EnemyRewindInfoList[playerPositions.Count - i].position;
            // begin player rewind movement
            var timer = 0f;
            while (timer < 1f / RewindSpeed)
            {
                rewindDir = playerPositions[playerPositions.Count - i] - transform.position;
                isRewindMoving = rewindDir.magnitude > .5f; // check magnitude to determine whether moving or not
                rewindDir = rewindDir.normalized; // get normalized vector for move
                timer += Time.deltaTime;
                yield return null;
            }
        }
        isRewindMoving = false;
        // clean up player positions list
        playerPositions.RemoveRange(playerPositions.Count - rewindTime, rewindTime);
        // set new position log time
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        // unpause game
        Services.TimedGameMode.TogglePause();
        // stop rewind effect
        rewindBlit.SetActive(false);
        // end rewind
        isRewinding = false;
        canRewind = false;
        StartCoroutine(RewindCooldown());
    }

    void OnDisable()
    {
        rewindBlit.SetActive(false);
    }
}
