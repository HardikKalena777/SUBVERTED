using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Scripts")]
    InputManager inputManager;
    PlayerManager playerManager;
    AnimationManager animationManager;

    [Header("Componenets")]
    Vector3 moveDirection;
    Transform cameraObj;
    Rigidbody playerRigidBody;

    [Header("Variables")]

    [Header("Falling"), Space(5)]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset = 0.5f;
    public LayerMask groundLayer;

    [Header("Movement Flags"), Space(5)]
    public bool isRunning;
    public bool isGrounded;
    public bool isJumping;
    public bool allowJog;
    public bool isJogging;
    public bool isWalking;

    [Header("Movement Speeds"),Space(5)]
    public float walkSpeed = 1.5f;
    public float jogSpeed = 5f;
    public float runSpeed = 7f;
    public float rotationSpeed = 10f;

    [Header("Jump Speeds")]
    public float jumpHeight = 3f;
    public float gravityIntensity = -15f;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        animationManager = GetComponent<AnimationManager>();
        playerRigidBody = GetComponent<Rigidbody>();
        cameraObj = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if(isJumping) 
            return;
        HandleFallingAndLanding();

        if(playerManager.isInteracting) 
            return;

        moveDirection = cameraObj.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObj.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if(isRunning && allowJog)
        {
            moveDirection = moveDirection * runSpeed;
        }
        else
        {
            if(inputManager.moveAmount > 0.5f && allowJog)
            {
                moveDirection = moveDirection * jogSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkSpeed;
            }
        }

        Vector3 movementVelocity = moveDirection;
        playerRigidBody.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if(isJumping) return;

        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObj.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObj.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if(targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        Vector3 targetPosition;
        rayCastOrigin.y += rayCastHeightOffset;
        targetPosition = transform.position;

        
        if (Physics.SphereCast(rayCastOrigin, 0.2f, Vector3.down, out hit, rayCastHeightOffset + 0.1f, groundLayer))
        {
            if (!isGrounded && playerManager.isInteracting)
            {
                animationManager.PlayTargetAnimation("Land", true, 0.1f);
            }

            Vector3 rayCastHitPoint = hit.point;
            targetPosition.y = rayCastHitPoint.y;
            inAirTimer = 0f;
            isGrounded = true; 
        }
        else
        {
            if (!isJumping) 
            {
                if (!playerManager.isInteracting)
                {
                    animationManager.PlayTargetAnimation("Fall", true, 0.2f);
                }
            }

            inAirTimer += Time.deltaTime;
            isGrounded = false; 
        }

        if(isGrounded && !isJumping)
        {
            if(playerManager.isInteracting || inputManager.moveAmount > 0)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.01f);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }

    public void HandleJumping()
    {
        if(isGrounded)
        {
            isGrounded = false;
            animationManager.animator.SetBool("IsJumping",true);
            animationManager.PlayTargetAnimation("Jump",false,0.1f);

            float jumpVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpVelocity;
            playerRigidBody.linearVelocity = playerVelocity;
        }
    }
}
