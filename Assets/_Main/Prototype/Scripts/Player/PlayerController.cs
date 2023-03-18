using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Stevehuu;

public enum camDirection
{ South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast, Count };

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    // input
    public PrototypePlayerInput input;

    // direction
    private Vector3 targetDirection;
    // cam based direction
    [Header("Camera Switch")]
    public camDirection currentCamDirection;
    [HideInInspector] public bool isInvertedControls;
    private bool playerLeft; // for changing animation of running left & right
    [SerializeField] private Transform mainCam;
    [SerializeField] private float camLerpSpeed;
    // new camera
    private Vector2 currentMouseInput;

    // movement
    private Vector2 currentMovementInput;
    private bool isMoving;
    private bool isSprinting;
    private float moveSpeed;
    
    [Header("Movement")]
    [Tooltip("Default walking speed")]
    public float walkSpeed = 5f;
    [Tooltip("Default sprinting speed")]
    public float sprintSpeed = 8f;
    [Space(10)]
    [Tooltip("How fast the character accelerrats")]
    public float speedChangeRate = 10f;
    [Tooltip("How fast the character rotates")]
    [Range(0.0f, 0.5f)]
    public float rotationSpeed = 0.1f;

    // dash
    private bool isDashing;
    private bool canDash;
    [Header("Dash")]
    [Tooltip("How quickly can the player dash")]
    public float dashDistance = 2f;
    [Tooltip("How long the dash lasts")]
    [Range(0, .5f)]
    public float dashDuration = .1f;
    [Tooltip("How long before next dash")]
    public float dashCooldown = .5f;
    
    // animations
    [Header("Animation")]
    Animator _animator;

    // test
    private float cangle = 0;

    [Header("Test")]
    [SerializeField] private bool isLookingBack;

    // enabling input
    void OnEnable()
    {
        input.Player.Enable();
    }
    void OnDisable()
    {
        input.Player.Disable();
    }

    void AssignComponents()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    void SubscribeInputEvents()
    {
        input = new PrototypePlayerInput();
        // input events
        // walking & running
        input.Player.Move.performed += OnMovementInput;
        input.Player.Move.canceled += OnMovementInput;
        input.Player.Sprint.performed += context => isSprinting = context.ReadValueAsButton();
        // dashing
        canDash = true;
        input.Player.Dash.performed += Dash;
        // camera change
        input.Player.ChangeCameraLeft.performed += ctx => ChangeCamera("Left");
        input.Player.ChangeCameraRight.performed += ctx => ChangeCamera("Right");
        input.Player.LookBack.performed += ctx => OnLookBack(true);
        input.Player.LookBack.canceled += ctx => OnLookBack(false);
        // pausing
        input.Player.Pause.performed += ctx => Services.TimedGameMode.TogglePause();
    }

    void LoadControlSettings()
    {
        isInvertedControls = PlayerPrefs.GetInt("InvertCam") == 1;
    }

    void Awake()
    {
        Services.PlayerController = this;
        LoadControlSettings();
        SubscribeInputEvents();
        AssignComponents();
        currentCamDirection = camDirection.South;
    }

    // change animation state
    void HandleAnimation()
    {
        if (GameManager.isGamePaused || GameManager.isGameEnded) return; // do not run on pause
        var currentState = _animator.GetCurrentAnimatorStateInfo(0).ToString();
        if (isMoving) { AnimationChange.ChangeAnimationState(_animator, currentState, "NewRun", true, currentMovementInput.x < 0); _animator.speed = _characterController.velocity.magnitude * .2f; }
        else { AnimationChange.ChangeAnimationState(_animator, currentState, "NewIdle", false, currentMovementInput.x < 0); _animator.speed = 1; }
    }

    void Update()
    {
        HandleAnimation();
    }

    void FixedUpdate()
    {
        Move();
    }

    // ACTION METHODS
    // movement
    private void OnMovementInput(InputAction.CallbackContext context)                                    
    {                                                                                            
        currentMovementInput = context.ReadValue<Vector2>();                                     
        isMoving = currentMovementInput.x != 0 || currentMovementInput.y != 0;                   
    }
    // movement (called per frame)
    private void Move()
    {
        // get target speed based on sprint or walk
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        if (!isMoving) targetSpeed = 0;
        // get input magnitude, cap at 1
        float inputMagnitude = currentMovementInput.magnitude;
        if (inputMagnitude > 1f) inputMagnitude = 1f;

        // get current horizontal speed
        float currentHorizontalSpeed = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z).magnitude;
        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - .1f || currentHorizontalSpeed > targetSpeed + .1f)
        {
            moveSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, speedChangeRate * Time.deltaTime);
            // round speed to 3 decimal places
            moveSpeed = Mathf.Round(moveSpeed * 1000f) / 1000f;
        }
        else
        {
            moveSpeed = targetSpeed;
        }
        
        // create move vector
        Vector3 direct = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y).normalized;
        float targetAngle = Mathf.Atan2(direct.x, direct.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y - (isLookingBack ? 180 : 0);
        Vector3 move = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        // gravity
        var gravityVector = new Vector3(0, _characterController.isGrounded ? 0 : -1, 0);

        // rotate player when moving
        if (isMoving)
        {
            // set player direction to move direction, ignore Y
            Rotate(new Vector3(move.x, 0, move.z), rotationSpeed);
        }
        // move player
        if (isDashing) _characterController.Move(transform.forward * Time.deltaTime * (dashDistance / dashDuration));
        else _characterController.Move((move.normalized + gravityVector) * Time.deltaTime * moveSpeed);
    }
    // movement rotation
    private void Rotate(Vector3 rotateTo, float rate)
    {
        // parse direction to quaternion
        Quaternion targetRotation = Quaternion.LookRotation(rotateTo);
        // rotate player
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rate);
    }

    // dash
    private void Dash(InputAction.CallbackContext context)
    {
        if (!isDashing && canDash)
        {
            StartCoroutine(Dashing());
        }
    }
    private IEnumerator Dashing()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashDuration);
        StartCoroutine(DashCooling());
        canDash = false;
        isDashing = false;
    }
    private IEnumerator DashCooling()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // change camera
    private void ChangeCamera(string direction)
    {
        if (isLookingBack || GameManager.isGamePaused || GameManager.isGameEnded) return; // prevent camera change when looking back
        var camIndex = (int)currentCamDirection;
        switch (direction)
        {
            case "Right":
                camIndex += isInvertedControls ? -1 : 1;
                break;
            case "Left":
                camIndex += isInvertedControls ? 1 : -1;
                break;
        }
        if (camIndex > (int)camDirection.Count - 1) camIndex = 0;
        if (camIndex < 0) camIndex = (int)camDirection.Count - 1;
        currentCamDirection = (camDirection)camIndex;
        if (camSwitchRoutineInstance != null) StopCoroutine(camSwitchRoutineInstance);
        camSwitchRoutineInstance = StartCoroutine(CameraSwitchRoutine(GetTargetAngle()));
    }

    private Coroutine camSwitchRoutineInstance;
    private IEnumerator CameraSwitchRoutine(float angle)
    {
        var lerpElapsed = 0f;
        while (lerpElapsed < camLerpSpeed)
        {
            mainCam.eulerAngles = 
                new Vector3(
                    mainCam.eulerAngles.x,
                    Mathf.LerpAngle(mainCam.eulerAngles.y, 
                        angle, lerpElapsed / camLerpSpeed),
                    0);
            lerpElapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    float GetTargetAngle()
    {
        float angle = currentCamDirection switch
        {
            camDirection.South => 0,
            camDirection.SouthWest => 45,
            camDirection.West => 90,
            camDirection.NorthWest => 135,
            camDirection.North => 180,
            camDirection.NorthEast => -135,
            camDirection.East => -90,
            camDirection.SouthEast => -45,
        };

        angle += isLookingBack ? 180 : 0;
        return angle;
    }

    // look back logic
    void OnLookBack(bool isActive)
    {
        isLookingBack = isActive;
        if (camSwitchRoutineInstance != null) StopCoroutine(camSwitchRoutineInstance);
        mainCam.eulerAngles = new Vector3(mainCam.eulerAngles.x, GetTargetAngle(), 0);
    }
}
