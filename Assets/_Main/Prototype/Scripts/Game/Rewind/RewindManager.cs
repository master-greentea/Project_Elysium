using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using UnityEngine;
using UnityEngine.Analytics;

public class RewindManager : MonoBehaviour
{
    [SerializeField] private CameraEffects CameraEffects;
    [Range(3, 10)] public int maxRewindTimeAmount;
    [SerializeField] private bool canRewindOnStart;
    [SerializeField] private bool rewindCooldownIsDoubleTime;
    [SerializeField] private float rewindCooldown;
    
    public static bool isRewinding;
    public static bool canRewind { get; private set; }
    public static int setRewindTime { get; set; }
    public const int RewindSpeed = 2;
    private PlayerController _playerController;
    // positions
    private CharacterController _characterController;
    private static List<Vector3> PlayerPositionList;
    private static int lastSecond;
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
        _playerController = GetComponent<PlayerController>();
        PlayerPositionList = new List<Vector3>(maxRewindTimeAmount);
        CameraRewindInfoList = new List<CameraRewindInfo>();
        EnemyRewindInfoList = new List<EnemyRewindInfo>(maxRewindTimeAmount);
        lastSecond = -1;
        if (canRewindOnStart) {canRewind = true; Services.VHSDisplay.DisplayNotification("Rewind: Ready");}
        else StartCoroutine(RewindCooldown());
    }

    IEnumerator RewindCooldown()
    {
        yield return new WaitForSeconds(rewindCooldownIsDoubleTime ? setRewindTime * 2 : rewindCooldown);
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
        _characterController.Move(rewindDir * (Time.deltaTime * _playerController.walkSpeed * RewindSpeed));
    }

    void LogInformationPerSecond()
    {
        // do not log position if is rewinding or skipping
        if (isRewinding || SkipManager.isTimeSkipping) return;
        // check if time has passed
        if (Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime) <= lastSecond) return;
        // log player position, enemy position, and enemy state
        LogPlayerPosition(transform.position);
        LogEnemyPosition(Services.EnemyAgent.transform.position);
        // clean up camera log list
        foreach (var (value, i) in CameraRewindInfoList.Select((value, i) => ( value, i )))
        {
            if (value.timeStampInSeconds < TimedGameMode.survivedTime - maxRewindTimeAmount) continue;
            CameraRewindInfoList.RemoveRange(0, i);
            break;
        }
    }

    public static void LogPlayerPosition(Vector3 position)
    {
        // log player position
        if (PlayerPositionList.Count < PlayerPositionList.Capacity)
        {
            PlayerPositionList.Add(position);
        }
        else
        {
            PlayerPositionList.RemoveAt(0);
            PlayerPositionList.Add(position);
        }
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
    }

    public static void LogEnemyPosition(Vector3 position)
    {
        // log enemy position and enemy state
        if (EnemyRewindInfoList.Count < EnemyRewindInfoList.Capacity)
        {
            EnemyRewindInfoList.Add(new EnemyRewindInfo(position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        else
        {
            EnemyRewindInfoList.RemoveAt(0);
            EnemyRewindInfoList.Add(new EnemyRewindInfo(position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
    }
    
    public static void LogCamera(camDirection direction)
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
            if (camLog.timeStampInSeconds != Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime))
                continue;
            _playerController.SwitchCamera(camLog.loggedDirection, .3f);
            CameraRewindInfoList.Remove(camLog);
            break;
        }
    }

    public IEnumerator Rewind(int rewindSeconds)
    {
        isRewinding = true;
        // restart time scale for move
        Time.timeScale = 1;
        // turn off menu
        VHSButtonsManager.canvas.enabled = false;
        Services.VHSDisplay.DisplayNotification("");
        // begin rewind effect
        CameraEffects.ToggleRewind(true);
        // begin enemy rewind
        Services.EnemyAgent.EnemyStateMachine.ChangeState(EnemyStateId.Rewind);
        // for each second in rewind
        for (int i = 1; i <= rewindSeconds; i++)
        {
            // play logged camera movement
            CheckCameraPositionLog();
            // set enemy rewind to last position
            int positionIndex = PlayerPositionList.Count - i;
            positionIndex = Mathf.Clamp(positionIndex, 0, setRewindTime - 1);
            RewindState.positionToRewindTo = EnemyRewindInfoList[positionIndex].position;
            // begin player rewind movement
            var timer = 0f;
            while (timer < 1f / (RewindSpeed + .1f))
            {
                rewindDir = PlayerPositionList[positionIndex] - transform.position;
                isRewindMoving = rewindDir.magnitude > .1f; // check magnitude to determine whether moving or not
                rewindDir = rewindDir.normalized; // get normalized vector for move
                timer += Time.deltaTime;
                yield return null;
            }
            // rewind time
            TimedGameMode.AddTime(-1);
        }
        isRewindMoving = false;
        // clean up player positions list
        PlayerPositionList.RemoveRange(PlayerPositionList.Count - setRewindTime, setRewindTime);
        // set new position log time
        lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.survivedTime);
        // stop rewind effect
        CameraEffects.ToggleRewind(false);
        // unpause game after a short break
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(.5f);
        // unpause game
        Services.TimedGameMode.TogglePause();
        // end rewind
        isRewinding = false;
        canRewind = false;
        StartCoroutine(RewindCooldown());
    }
}
