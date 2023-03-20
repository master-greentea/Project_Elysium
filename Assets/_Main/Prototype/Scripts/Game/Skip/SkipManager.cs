using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkipManager : MonoBehaviour
{
    [SerializeField] private CameraEffects cameraEffects;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] private float timeSkipAmount;
    public static bool isTimeSkipping;
    private CinemachineFramingTransposer virtualCamFramingTransposer;
    private CharacterController _characterController;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        virtualCamFramingTransposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            StartCoroutine(TimeSkip());
        }
    }

    void SetCameraDamping(float setTo)
    {
        virtualCamFramingTransposer.m_XDamping =
            virtualCamFramingTransposer.m_YDamping =
                virtualCamFramingTransposer.m_ZDamping = setTo;
    }

    private IEnumerator TimeSkip()
    {
        isTimeSkipping = true;
        SetCameraDamping(0);
        cameraEffects.TriggerTimeSkip();
        // skip player
        if (PlayerController.isMoving) _characterController.Move(transform.forward * 5);
        // skip time
        // TODO: debug rewind when time skipped
        
        TimedGameMode.TimeSkip(timeSkipAmount);
        // skip enemy
        Services.EnemyAgent.transform.position += Services.EnemyAgent.transform.forward * 5;
        yield return new WaitForSeconds(.1f);
        SetCameraDamping(1);
        isTimeSkipping = false;
    }
}
