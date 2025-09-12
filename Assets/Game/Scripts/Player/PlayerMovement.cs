using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private float walkSprintTransition;
    [SerializeField]
    private float rotationSmoothTime = 0.1f;

    [SerializeField]
    private Vector3 upperStepOffset;
    [SerializeField]
    private float stepCheckerDistance;
    [SerializeField]
    private float stepForce;

    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Transform groundDetector;
    [SerializeField]
    private float detectorRadius;
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private Transform climbDetector;
    [SerializeField]
    private float climbCheckDistance;
    [SerializeField]
    private LayerMask climableLayer;
    [SerializeField]
    private Vector3 climbOffset;
    [SerializeField]
    private float climbSpeed;

    [SerializeField]
    private Transform camera;
    [SerializeField]
    private CameraManager cameraManager;

    private float rotationSmoothVelocity;

    private float speed;
    private bool isGrounded;

    private PlayerStance playerStance;

    private Rigidbody rb;
    private Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        speed = walkSpeed;
        playerStance = PlayerStance.Stand;
        HideAndLockCursor();

    }
    private void Start()
    {
        inputManager.OnMoveInput += Move;
        inputManager.OnSprintInput += Sprint;
        inputManager.OnJumpInput += Jump;
        inputManager.OnClimbInput += StartClimb;    
        inputManager.OnCancelClimb += CancelClimb;
        cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void OnDestroy()
    {
        inputManager.OnMoveInput -= Move;
        inputManager.OnSprintInput -= Sprint;
        inputManager.OnJumpInput -= Jump;
        inputManager.OnClimbInput -= StartClimb;
        inputManager.OnCancelClimb -= CancelClimb;
        cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = playerStance == PlayerStance.Climb;
        if (isPlayerStanding)
        {
            switch (cameraManager.cameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + camera.transform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        rb.AddForce(movementDirection * speed * Time.deltaTime);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, camera.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    rb.AddForce(movementDirection * speed * Time.deltaTime);

                    break;
            }
            Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
            animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
            animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);


        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
             movementDirection = horizontal + vertical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);
        }

    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (speed < sprintSpeed)
            {
                speed = speed + walkSprintTransition * Time.deltaTime;
            }
        }

        else
        {
            if (speed > sprintSpeed)
            {
                speed = speed - walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            Vector3 jumpDirection = Vector3.up; // sama seperti new Vector3(...;
            rb.AddForce(jumpDirection * jumpForce * Time.deltaTime);
            animator.SetTrigger("Jump");
        }

        
    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundDetector.position, detectorRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(groundDetector.position, transform.forward, stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(groundDetector.position + upperStepOffset, transform.forward, stepCheckerDistance);

        if (isHitLowerStep && !isHitUpperStep)
        {
            rb.AddForce(0, stepForce * Time.deltaTime, 0);
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbing = Physics.Raycast(climbDetector.position, transform.forward,out RaycastHit hit, climbCheckDistance, climableLayer);
        bool isNotClimbing = playerStance != PlayerStance.Climb;
        if(isInFrontOfClimbing && isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * climbOffset.z) + (Vector3.up * climbOffset.y);
            transform.position = hit.point - offset;
            playerStance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = climbSpeed;
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            cameraManager.SetTPSFieldOfView(70);
        }
    }

    private void CancelClimb()
    {
        if (playerStance == PlayerStance.Climb)
        {
            playerStance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward;
            speed = walkSpeed;
            cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            cameraManager.SetTPSFieldOfView(60);
        }
    }

    private void ChangePerspective()
    {
        animator.SetTrigger("ChangePerspective");
    }

}
