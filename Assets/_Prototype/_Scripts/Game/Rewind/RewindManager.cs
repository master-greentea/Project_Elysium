using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    [SerializeField] private CameraEffects cameraEffects;
    private PlayerController playerController;
    private CharacterController characterController;
    [Range(3, 10)] public int maxRewindTimeAmount;
    [SerializeField] private bool canRewindOnStart;
    [SerializeField] private bool rewindCooldownIsDoubleTime;
    [SerializeField, ConditionalHide("rewindCooldownIsDoubleTime", true)] private float rewindCooldown;
    
    public static bool isRewinding;
    public static bool CanRewind { get; private set; }
    public static int SetRewindTime { get; set; }
    public const int RewindSpeed = 2;
    // positions
    private static List<Vector3> _playerPositionList;
    private static int _lastSecond;
    private Vector3 rewindDir;
    public static bool isRewindMoving;
    // camera input
    private struct CameraRewindInfo
    {
        public readonly int timeStampInSeconds;
        public readonly CameraDirection loggedDirection;
        public CameraRewindInfo(CameraDirection _direction)
        {
            timeStampInSeconds = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
            loggedDirection = _direction;
        }
    }
    private static List<CameraRewindInfo> _cameraRewindInfoList;
    // enemy
    public struct EnemyRewindInfo
    {
        public readonly Vector3 position;
        public readonly EnemyStateId stateId;
        public EnemyRewindInfo(Vector3 _position, EnemyStateId _stateId)
        {
            position = _position;
            stateId = _stateId;
        }
    }
    public static List<EnemyRewindInfo> enemyRewindInfoList;

    void Awake()
    {
        Services.RewindManager = this;
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        _playerPositionList = new List<Vector3>(maxRewindTimeAmount);
        _cameraRewindInfoList = new List<CameraRewindInfo>();
        enemyRewindInfoList = new List<EnemyRewindInfo>(maxRewindTimeAmount);
        _lastSecond = -1;
        if (canRewindOnStart) {CanRewind = true; Services.VHSDisplay.DisplayNotification("Rewind: Ready");}
        else StartCoroutine(RewindCooldown());
    }

    IEnumerator RewindCooldown()
    {
        CanRewind = false;
        yield return new WaitForSeconds(rewindCooldownIsDoubleTime ? SetRewindTime * 2 : SetRewindTime + rewindCooldown);
        Services.VHSDisplay.DisplayNotification("Rewind: Ready");
        CanRewind = true;
    }
    
    void Update()
    {
        LogInformationPerSecond();
    }

    void FixedUpdate()
    {
        if (!isRewinding) return;
        // rewind player move
        characterController.Move(rewindDir * (Time.deltaTime * playerController.walkSpeed * RewindSpeed));
    }

    void LogInformationPerSecond()
    {
        // do not log position if is rewinding or skipping
        if (isRewinding || SkipManager.isTimeSkipping) return;
        // check if time has passed
        if (Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime) <= _lastSecond) return;
        // log player position, enemy position, and enemy state
        LogPlayerPosition(transform.position);
        if (Services.EnemyAgent) LogEnemyPosition(Services.EnemyAgent.transform.position);
        // clean up camera log list
        foreach (var (value, i) in _cameraRewindInfoList.Select((value, i) => ( value, i )))
        {
            if (value.timeStampInSeconds < TimedGameMode.SurvivedTime - maxRewindTimeAmount) continue;
            _cameraRewindInfoList.RemoveRange(0, i);
            break;
        }
    }

    public static void LogPlayerPosition(Vector3 position)
    {
        // log player position
        if (_playerPositionList.Count < _playerPositionList.Capacity)
        {
            _playerPositionList.Add(position);
        }
        else
        {
            _playerPositionList.RemoveAt(0);
            _playerPositionList.Add(position);
        }
        _lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
    }

    public static void LogEnemyPosition(Vector3 position)
    {
        // log enemy position and enemy state
        if (enemyRewindInfoList.Count < enemyRewindInfoList.Capacity)
        {
            enemyRewindInfoList.Add(new EnemyRewindInfo(position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        else
        {
            enemyRewindInfoList.RemoveAt(0);
            enemyRewindInfoList.Add(new EnemyRewindInfo(position, Services.EnemyAgent.EnemyStateMachine.currentState));
        }
        _lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
    }
    
    public static void LogCamera(CameraDirection direction)
    {
        // do not log position if is rewinding
        if (isRewinding) return;
        // create camera log
        CameraRewindInfo log = new CameraRewindInfo(direction);
        // check if there's already a log at the same second
        foreach (var camLog in _cameraRewindInfoList)
        {
            if (camLog.timeStampInSeconds != log.timeStampInSeconds) continue;
            // only keep the latest one if triggered multiple logs in one second
            _cameraRewindInfoList.Remove(camLog);
            break;
        }
        _cameraRewindInfoList.Add(log);
    }

    void RewindCameraPosition()
    {
        foreach (var camLog in _cameraRewindInfoList)
        {
            if (camLog.timeStampInSeconds != Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime))
                continue;
            playerController.SwitchCamera(camLog.loggedDirection, .3f);
            _cameraRewindInfoList.Remove(camLog);
            break;
        }
    }

    public IEnumerator Rewind(int rewindSeconds)
    {
        isRewinding = true;
        // restart time scale for move
        Time.timeScale = 1;
        // turn off menu
        PauseMenuManager.pauseCanvas.enabled = false;
        Services.VHSDisplay.DisplayNotification("");
        // begin rewind effect
        cameraEffects.ToggleRewind(true);
        // begin enemy rewind
        if (Services.EnemyAgent) Services.EnemyAgent.EnemyStateMachine.ChangeState(EnemyStateId.Rewind);
        // for each second in rewind
        for (int i = 1; i <= rewindSeconds; i++)
        {
            // play logged camera movement
            RewindCameraPosition();
            // set enemy rewind to last position
            int positionIndex = _playerPositionList.Count - i;
            positionIndex = Mathf.Clamp(positionIndex, 0, SetRewindTime - 1);
            if (Services.EnemyAgent) RewindState.positionToRewindTo = enemyRewindInfoList[positionIndex].position;
            // begin player rewind movement
            var timer = 0f;
            while (timer < 1f / (RewindSpeed + .1f))
            {
                rewindDir = _playerPositionList[positionIndex] - transform.position;
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
        _playerPositionList.RemoveRange(_playerPositionList.Count - SetRewindTime, SetRewindTime);
        // set new position log time
        _lastSecond = Services.VHSDisplay.GetFormattedSecond(TimedGameMode.SurvivedTime);
        // stop rewind effect
        cameraEffects.ToggleRewind(false);
        // unpause game after a short break
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(.5f);
        // unpause game
        Services.PauseMenuManager.TogglePause();
        // end rewind
        isRewinding = false;
        CanRewind = false;
        StartCoroutine(RewindCooldown());
    }
}
