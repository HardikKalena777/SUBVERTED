using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerLocomotion playerLocomotion;
    AnimationManager animationManager;

    [HideInInspector] public Vector2 movementInput;
    [HideInInspector] public float moveAmount;
    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public bool runInput;
    [HideInInspector] public bool jumpInput;

    private void Awake()
    {
        animationManager = GetComponent<AnimationManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable()
    {
        if(playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Run.performed += i => runInput = true;
            playerControls.PlayerActions.Run.canceled += i => runInput = false;

            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
        }
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleWalkInput();
        HandleJogInput();
        HandleRunInput();
        HandleJumpInput();
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        animationManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.isRunning,playerLocomotion.isJogging,playerLocomotion.isWalking);
    }

    private void HandleWalkInput()
    {
        if(!playerLocomotion.allowJog && moveAmount > 0)
        {
            playerLocomotion.isWalking = true;
        }
        else
        {
            playerLocomotion.isWalking = false;
        }
    }

    private void HandleJogInput()
    {
        if(playerLocomotion.allowJog && moveAmount > 0.5f && !playerLocomotion.isRunning)
        {
            playerLocomotion.isJogging = true;
        }
        else
        {
            playerLocomotion.isJogging = false;
        }
    }

    private void HandleRunInput()
    {
        if (runInput && moveAmount > 0.5f)
        {
            playerLocomotion.isRunning = true;
        }
        else
        {
            playerLocomotion.isRunning = false;
        }
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerLocomotion.HandleJumping();
        }
    }
}
