using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float crouchSpeed;
    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private float walkSprintTransition;
    [SerializeField]
    private float rotationSmoothTime;

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
    private Transform cameraA;
    [SerializeField]
    private CameraManager cameraManager;

    [SerializeField]
    private float glideSpeed;
    [SerializeField]
    private float airDrag;
    [SerializeField]
    private Vector3 glideRotationSpeed;
    [SerializeField]
    private float minGlideRotationX;
    [SerializeField]
    private float maxGlideRotationX;

    [SerializeField]
    private float resetComboInterval;
    private Coroutine resetCombo;

    [SerializeField]
    private Transform hitDetector;
    [SerializeField]
    private float hitDetectorRadius;
    [SerializeField]
    private LayerMask hitLayer;

    [SerializeField]
    private PlayerAudioManager playerAudioManager;

    private bool isPunching;
    private int combo = 0;

    private float rotationSmoothVelocity;

    private float speed;
    private bool isGrounded;
    private Vector3 rotationDegree = Vector3.zero;

    private PlayerStance playerStance;

    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider colliderCP;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        colliderCP = GetComponent<CapsuleCollider>();
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
        inputManager.OnCrouchInput += Crouch;
        inputManager.OnGlideInput += StartGlide;
        inputManager.OnPunchInput += Punch;
        inputManager.OnCancelGlide += CancelGlide;

        cameraManager.OnChangePerspective += ChangePerspective;

    }

    private void OnDestroy()
    {
        inputManager.OnMoveInput -= Move;
        inputManager.OnSprintInput -= Sprint;
        inputManager.OnJumpInput -= Jump;
        inputManager.OnClimbInput -= StartClimb;
        inputManager.OnCancelClimb -= CancelClimb;
        inputManager.OnCrouchInput -= Crouch;
        inputManager.OnGlideInput -= StartGlide;
        inputManager.OnPunchInput -= Punch;
        inputManager.OnCancelGlide -= CancelGlide;

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
        bool isPlayerCrouching = playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = playerStance == PlayerStance.Glide;
        if ((isPlayerStanding || isPlayerCrouching) && !isPunching)
        {
            switch (cameraManager.cameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + cameraA.transform.eulerAngles.y;
                        //float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);

                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        rb.AddForce(movementDirection * speed * Time.deltaTime);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, cameraA.eulerAngles.y, 0f);
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
            Vector3 horizontal = Vector3.zero;
            Vector3 vertical = Vector3.zero;

            Vector3 checkerLeftPosition = transform.position + (transform.up * 1) + (-transform.right * 0.25f);
            Vector3 checkerRightPosition = transform.position + (transform.up * 1) + (transform.right * 0.25f);
            Vector3 checkerUpPosition = transform.position + (transform.up * 1.9f);
            Vector3 checkerDownPosition = transform.position + (-transform.up * 0.25f);
            bool isAbleClimbLeft = Physics.Raycast(checkerLeftPosition, transform.forward, climbCheckDistance,climableLayer);
            bool isAbleClimbRight = Physics.Raycast(checkerRightPosition, transform.forward, climbCheckDistance,climableLayer);
            bool isAbleClimbUp = Physics.Raycast(checkerUpPosition,transform.forward, climbCheckDistance, climableLayer);
            bool isAbleClimbDown = Physics.Raycast(checkerDownPosition, transform.forward, climbCheckDistance, climableLayer);

            if((isAbleClimbLeft && (axisDirection.x < 0)) || (isAbleClimbRight && axisDirection.x > 0))
            {
                horizontal = axisDirection.x * transform.right;
            }
            if((isAbleClimbUp && (axisDirection.y > 0)) || (isAbleClimbDown && (axisDirection.y < 0)))
            {
                vertical = axisDirection.y * transform.up;
            }

            movementDirection = horizontal + vertical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);
            rb.AddForce(movementDirection * Time.deltaTime * climbSpeed);
            Vector3 velocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
            animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
            animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
        }
        else if (isPlayerGliding)
        {
            rotationDegree.x += glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, minGlideRotationX, maxGlideRotationX);
            rotationDegree.z += glideRotationSpeed.z * axisDirection.x * Time.deltaTime;

            rotationDegree.y += glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);
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
            if (speed > walkSpeed)
            {
                speed = speed - walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        if (isGrounded && !isPunching)
        {
            Vector3 jumpDirection = Vector3.up; // sama seperti new Vector3(...;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            animator.SetBool("IsJump", true);
            animator.SetBool("IsJump", false);
        }


    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundDetector.position, detectorRadius, groundLayer);
        animator.SetBool("IsGrounded", isGrounded);
        if (isGrounded)
        {
            CancelGlide();
        }
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
        bool isInFrontOfClimbing = Physics.Raycast(climbDetector.position, transform.forward, out RaycastHit hit, climbCheckDistance, climableLayer);
        bool isNotClimbing = playerStance != PlayerStance.Climb;
        if (isInFrontOfClimbing && isGrounded && isNotClimbing)
        {
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            Vector3 climablePoint = hit.collider.bounds.ClosestPoint(transform.position);
            Vector3 direction = (climablePoint - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);

            //collider.center = Vector3.up * 1.3f;
            Vector3 offset = (transform.forward * climbOffset.z) - (Vector3.up * climbOffset.y);
            transform.position = hit.point - offset;
            playerStance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = climbSpeed;
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            cameraManager.SetTPSFieldOfView(70);
            animator.SetBool("IsClimbing", true);
            rb.useGravity = false;
        }
    }

    private void CancelClimb()
    {
        if (playerStance == PlayerStance.Climb)
        {
            colliderCP.center = Vector3.up * 0.9f;
            playerStance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward;
            speed = walkSpeed;
            cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            cameraManager.SetTPSFieldOfView(60);
            animator.SetBool("IsClimbing", false);
        }
    }

    private void ChangePerspective()
    {
        animator.SetTrigger("ChangePerspective");
    }

    private void Crouch()
    {
        if (playerStance == PlayerStance.Stand)
        {
            playerStance = PlayerStance.Crouch;
            animator.SetBool("IsCrouch", true);
            speed = crouchSpeed;
            colliderCP.height = 1.3f;
            colliderCP.center = Vector3.up * 0.66f;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerStance = PlayerStance.Stand;
            animator.SetBool("IsCrouch", false);
            colliderCP.height = 1.8f;
            colliderCP.center = Vector3.up * 0.9f;
            speed = walkSpeed;

        }

    }

    private void Glide()
    {
        if (playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + airDrag);
            Vector3 forwardForce = transform.forward * glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            rb.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void StartGlide()
    {
        if (playerStance != PlayerStance.Glide && !isGrounded)
        {
            playerStance = PlayerStance.Glide;
           rotationDegree = transform.rotation.eulerAngles;
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            playerAudioManager.PlayGlideSfx();
            animator.SetBool("IsGliding", true);

        }
    }

    private void CancelGlide()
    {
        if (playerStance == PlayerStance.Glide)
        {
            playerStance = PlayerStance.Stand;
            
            cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            playerAudioManager.StopGlideSFX();
            animator.SetBool("IsGliding", false);
        }
    }

    private void Punch()
    {
        if (!isPunching && playerStance == PlayerStance.Stand && isGrounded)
        {
            isPunching = true;
            if (combo < 3)
            {
                combo = combo + 1;
            }
            else
            {
                combo = 1;
            }
            animator.SetInteger("Combo", combo);
            animator.SetTrigger("Punch");
        }

    }

    private void EndPunch()
    {
        isPunching = false;
        if (resetCombo != null)
        {
            StopCoroutine(resetCombo);
        }
        resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(resetComboInterval);
        combo = 0;

    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(hitDetector.position, hitDetectorRadius, hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
            }
        }

    }
}
