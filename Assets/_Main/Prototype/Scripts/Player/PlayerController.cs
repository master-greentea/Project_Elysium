using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum camDirection
{ South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast, Count };

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    private CharacterController _characterController;

    // input
    private PrototypePlayerInput input;
    private PlayerInput playerInput;

    // direction
    private Vector3 targetDirection;
    // cam based direction
    [Header("Camera Switch")]
    public camDirection currentCamDirection;
    private bool playerLeft; // for changing animation of running left & right

    [Category("CM Main")] [SerializeField]
    private Transform mainCam;

    [SerializeField] private float camLerpSpeed;
    // new camera
    private Vector2 currentMouseInput;

    // movement
    private Vector2 currentMovementInput;
    private bool isMoving;
    private bool isSprinting;
    private float moveSpeed;
    
    [Space(10)]
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
    [Space(10)]
    [Header("Dash")]
    [Tooltip("How quickly can the player dash")]
    public float dashDistance = 2f;
    [Tooltip("How long the dash lasts")]
    [Range(0, .5f)]
    public float dashDuration = .1f;
    [Tooltip("How long before next dash")]
    public float dashCooldown = .5f;
    
    // animations
    [Space(10)]
    [Header("Animation")]
    private AnimationChange _animationChange;
    Animator _animator;
    const string PLAYER_IDLE = "NewIdle";
    const string PLAYER_RUN = "NewRun";
    
    // test
    private float cangle = 0;

    [Space(10)] [Header("Test")] [SerializeField]
    private bool isActiveCamera;
    [SerializeField] private float cameraSpeed;
    private bool isLookingBack;
    
    // enabling input
    void OnEnable()
    {
        input.PlayerControls.Enable();
    }
    void OnDisable()
    {
        input.PlayerControls.Disable();
    }

    void AssignComponents()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _animationChange = GetComponent<AnimationChange>();
    }

    void SubscribeInputEvents()
    {
        input = new PrototypePlayerInput();
        // input events
        // walking & running
        input.PlayerControls.Move.performed += OnMovementInput;
        input.PlayerControls.Move.canceled += OnMovementInput;
        input.PlayerControls.Sprint.performed += context => isSprinting = context.ReadValueAsButton();
        // dashing
        canDash = true;
        input.PlayerControls.Dash.performed += Dash;
        // camera change
        input.PlayerControls.ChangeCameraLeft.performed += context => ChangeCamera("Left");
        input.PlayerControls.ChangeCameraRight.performed += context => ChangeCamera("Right");
        input.PlayerControls.LookBack.performed += context => OnLookBack();
        input.PlayerControls.LookBack.canceled += context => OnLookBackStop();
    }

    void Awake()
    {
        Instance = this;
        SubscribeInputEvents();
        AssignComponents();
        currentCamDirection = camDirection.South;
    }

    // change animation state
    void HandleAnimation()
    {
        if (isMoving) { _animationChange.ChangeAnimationState(_animator, PLAYER_RUN, true, currentMovementInput.x < 0); _animator.speed = _characterController.velocity.magnitude * .2f; }
        else { _animationChange.ChangeAnimationState(_animator, PLAYER_IDLE, false, currentMovementInput.x < 0); _animator.speed = 1; }
    }

    void Update()
    {
        HandleAnimation();
        DirectionSwitch(isActiveCamera);
        Move();
    }

    void FixedUpdate()
    {

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
        int camIndex = (int)currentCamDirection;
        if (direction == "Right")
        {
            camIndex--;
            if (camIndex < 0) camIndex = (int)camDirection.Count - 1;
        }
        if (direction == "Left")
        {
            camIndex++;
            if (camIndex > (int)camDirection.Count - 1) camIndex = 0;
        }
        currentCamDirection = (camDirection)camIndex;
    }
    
    void DirectionSwitch(bool isActiveCamera)
    {
        float angle = 0;
        switch (currentCamDirection)
        {
            case camDirection.South:
                angle = 0;
                break;
            case camDirection.SouthWest:
                angle = 45;
                break;
            case camDirection.West:
                angle = 90;
                break;
            case camDirection.NorthWest:
                angle = 135;
                break;
            case camDirection.North:
                angle = 180;
                break;
            case camDirection.NorthEast:
                angle = -135;
                break;
            case camDirection.East:
                angle = -90;
                break;
            case camDirection.SouthEast:
                angle = -45;
                break;
            default:
                angle = 0;
                break;
        }
        
        angle += isLookingBack ? 180 : 0;

        if (isActiveCamera)
        {
            var mouseMovement = Mouse.current.delta.ReadValue();
            var yRotation = mouseMovement.x * Time.deltaTime * cameraSpeed;
            mainCam.eulerAngles = new Vector3(mainCam.eulerAngles.x, yRotation, 0);
        }
        else
        {
            mainCam.eulerAngles = new Vector3(
                mainCam.eulerAngles.x,
                Mathf.LerpAngle(mainCam.eulerAngles.y, 
                    angle, Time.deltaTime * (isLookingBack ? camLerpSpeed * 3.5f : camLerpSpeed)),
                0);
        }

        Cursor.lockState = isActiveCamera ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void OnLookBack()
    {
        isLookingBack = true;
    }
    void OnLookBackStop()
    {
        isLookingBack = false;
    }
}
