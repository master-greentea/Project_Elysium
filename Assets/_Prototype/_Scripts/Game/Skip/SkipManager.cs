using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Enemies;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkipManager : MonoBehaviour
{
    [SerializeField] private CameraEffects cameraEffects;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] [Range(2, 6)] public int timeSkipAmount;
    private float playerSkipDistance;
    private float enemySkipDistance;
    public static bool isTimeSkipping;
    public static bool canTimeSkip { get; private set; }
    private CinemachineFramingTransposer virtualCamFramingTransposer;
    private CharacterController _characterController;

    void Awake()
    {
        Services.SkipManager = this;
        _characterController = GetComponent<CharacterController>();
        virtualCamFramingTransposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        canTimeSkip = true;
    }
    
    IEnumerator TimeSkipCooldown()
    {
        yield return new WaitForSeconds(15f);
        Services.VHSDisplay.DisplayNotification("Rewind: Ready");
        canTimeSkip = true;
    }
    
    void Update()
    {
        // debug skip
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            // StartCoroutine(TimeSkip());
        }
    }

    void SetCameraDamping(float setTo)
    {
        virtualCamFramingTransposer.m_XDamping =
            virtualCamFramingTransposer.m_YDamping =
                virtualCamFramingTransposer.m_ZDamping = setTo;
    }

    void CalculateEnemySkipInfo(out Transform t, out float distance)
    {
        var enemyAgent = Services.EnemyAgent;
        t = enemyAgent.transform;
        var enemyDestination = enemyAgent.navMeshAgent.destination;
        // calculate distance
        distance = timeSkipAmount * enemyAgent.navMeshAgent.speed;
        distance = (enemyDestination - t.position).magnitude < distance
            ? (enemyDestination - t.position).magnitude
            : distance;
    }

    public IEnumerator TimeSkip()
    {
        isTimeSkipping = true;
        // instantly shift camera
        SetCameraDamping(0);
        cameraEffects.TriggerTimeSkip();
        CalculateEnemySkipInfo(out var et, out enemySkipDistance);
        playerSkipDistance = timeSkipAmount * PlayerController.PlayerCurrentSpeed;
        // for each second that is skipped
        for (int i = 1; i <= timeSkipAmount; i++)
        {
            // move player
            if (PlayerController.isMoving) _characterController.Move(
                transform.forward * (playerSkipDistance / timeSkipAmount));
            // move enemy
            if (EnemyAgent.isMoving) Services.EnemyAgent.navMeshAgent.Move(
                et.forward * (enemySkipDistance / timeSkipAmount));
            // log player and enemy positions for rewind to keep time consistent
            RewindManager.LogPlayerPosition(transform.position);
            RewindManager.LogEnemyPosition(et.position);
            // skip time
            TimedGameMode.AddTime(1);
        }
        yield return new WaitForSecondsRealtime(.1f);
        // reset camera to smooth movement
        SetCameraDamping(1);
        isTimeSkipping = false;
        canTimeSkip = false;
        StartCoroutine(TimeSkipCooldown());
        // change to chase instantly
        Services.EnemyAgent.EnemyStateMachine.ChangeState(EnemyStateId.Chase);
    }
}
