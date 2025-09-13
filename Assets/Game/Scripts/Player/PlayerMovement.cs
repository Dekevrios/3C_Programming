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

    private bool isPunching;
    private int combo = 0;

    private float rotationSmoothVelocity;

    private float speed;
    private bool isGrounded;

    private PlayerStance playerStance;

    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider collider;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
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
        inputManager.OnGlideInput -= CancelGlide;
        inputManager.OnPunchInput -= Punch;

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
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + camera.transform.eulerAngles.y;
                        //float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);

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
            rb.AddForce(movementDirection * Time.deltaTime * climbSpeed);
            Vector3 velocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
            animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
            animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
        }
        else if (isPlayerGliding)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x += Mathf.Clamp(rotationDegree.x, minGlideRotationX, maxGlideRotationX);
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
            collider.center = Vector3.up * 1.3f;
            Vector3 offset = (transform.forward * climbOffset.z) + (Vector3.up * climbOffset.y);
            transform.position = hit.point - offset;
            playerStance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = climbSpeed;
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            cameraManager.SetTPSFieldOfView(70);
            animator.SetBool("IsClimbing", true);
        }
    }

    private void CancelClimb()
    {
        if (playerStance == PlayerStance.Climb)
        {
            collider.center = Vector3.up * 0.9f;
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
            collider.height = 1.3f;
            collider.center = Vector3.up * 0.66f;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerStance = PlayerStance.Stand;
            animator.SetBool("IsCrouch", false);
            collider.height = 1.8f;
            collider.center = Vector3.up * 0.9f;
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
            animator.SetBool("IsGlide", true);
            cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);

        }
    }

    private void CancelGlide()
    {
        if (playerStance == PlayerStance.Glide)
        {
            playerStance = PlayerStance.Stand;
            animator.SetBool("IsGlide", false);
            cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
        }
    }

    private void Punch()
    {
        if (!isPunching && playerStance == PlayerStance.Stand)
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
