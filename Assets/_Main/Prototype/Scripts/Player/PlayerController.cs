using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public enum camDirection
{
    South,
    West,
    North,
    East
};

public class PlayerController : MonoBehaviour
{
    [SerializeField] public CharacterController characterController;

    // input
    PrototypePlayerInput input;

    // direction
    private Vector3 targetDirection;
    // cam based direction
    [Header("Camera Switch")]
    public camDirection currentCamDirection;
    private bool playerLeft; // for changing animation of running left & right
    private Vector3 playerCamBasedMove; // for changing direction of player move

    [Category("Directional Cameras")] [SerializeField]
    private GameObject[] directionCams;

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
    [SerializeField] private AnimationChange animationChange;
    Animator animator;
    const string PLAYER_IDLE = "NewIdle";
    const string PLAYER_RUN = "NewRun";

    void Awake()
    {
        input = new PrototypePlayerInput();
        animator = GetComponent<Animator>();

        // input events
        // walking & running
        input.PlayerControls.Move.performed += OnMovementInput;
        input.PlayerControls.Move.canceled += OnMovementInput;
        input.PlayerControls.Sprint.performed += context => isSprinting = context.ReadValueAsButton();
        // dashing
        canDash = true;
        input.PlayerControls.Dash.performed += context => Dash();
        // camera change
        input.PlayerControls.ChangeCameraLeft.performed += context => ChangeCamera("Left");
        input.PlayerControls.ChangeCameraRight.performed += context => ChangeCamera("Right");

        currentCamDirection = camDirection.South;
    }
    // event callback for Move
    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMoving = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    // change animation state
    void HandleAnimation()
    {
        if (isMoving) { animationChange.ChangeAnimationState(animator, PLAYER_RUN, true, playerLeft); animator.speed = characterController.velocity.magnitude * .2f; }
        else { animationChange.ChangeAnimationState(animator, PLAYER_IDLE, false, playerLeft); animator.speed = 1; }
    }

    void Update()
    {
        HandleAnimation();
        Move();
        DirectionSwitch();
    }

    // rotation
    public void Rotate(Vector3 rotateTo, float rate)
    {
        // parse direction to quaternion
        Quaternion targetRotation = Quaternion.LookRotation(rotateTo);
        // rotate player
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rate);
    }

    // walking & running
    private void Move()
    {
        // get target speed based on sprint or walk
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        if (!isMoving) targetSpeed = 0;
        // get input magnitude, cap at 1
        float inputMagnitude = currentMovementInput.magnitude;
        if (inputMagnitude > 1f) inputMagnitude = 1f;
        // create move vector
        Vector3 move = playerCamBasedMove;

        // get current horizontal speed
        float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
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

        // rotate player when moving
        if (isMoving)
        {
            // set player direction to move direction, ignore Y
            Rotate(new Vector3(move.x, 0, move.z), rotationSpeed);
        }
        // move player
        if (isDashing) characterController.Move(transform.forward * Time.deltaTime * (dashDistance / dashDuration));
        else characterController.Move(move.normalized * Time.deltaTime * moveSpeed);
    }

    // dashing
    private void Dash()
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

    // enabling input
    void OnEnable()
    {
        input.PlayerControls.Enable();
    }
    void OnDisable()
    {
        input.PlayerControls.Disable();
    }
    
    // change camera
    private void ChangeCamera(string direction)
    {
        // turn off all cams first
        for (int i = 0; i < directionCams.Length; i++)
        { directionCams[i].SetActive(false); }

        int camIndex = (int)currentCamDirection;
        if (direction == "Right")
        {
            camIndex--;
            if (camIndex < 0) camIndex = directionCams.Length - 1;
            currentCamDirection = (camDirection)camIndex;
        }
        if (direction == "Left")
        {
            camIndex++;
            if (camIndex > directionCams.Length - 1) camIndex = 0;
            currentCamDirection = (camDirection)camIndex;
        }
        
        directionCams[(int)currentCamDirection].SetActive(true);
    }
    
    // camera based direction switch
    void DirectionSwitch()
    {
        switch (currentCamDirection)
        {
            case camDirection.South:
                playerLeft = transform.forward.x < 0;
                playerCamBasedMove = new Vector3(currentMovementInput.x, 0, currentMovementInput.y);
                break;
            case camDirection.West:
                playerLeft = transform.forward.z > 0;
                playerCamBasedMove = new Vector3(currentMovementInput.y, 0, -currentMovementInput.x);
                break;
            case camDirection.North:
                playerLeft = transform.forward.x > 0;
                playerCamBasedMove = new Vector3(-currentMovementInput.x, 0, -currentMovementInput.y);
                break;
            case camDirection.East:
                playerLeft = transform.forward.z < 0;
                playerCamBasedMove = new Vector3(-currentMovementInput.y, 0, currentMovementInput.x);
                break;
        }
    }
}
