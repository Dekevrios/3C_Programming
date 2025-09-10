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
    private InputManager inputManager;
    [SerializeField]
    private Transform groundDetector;
    [SerializeField]
    private float detectorRadius;
    [SerializeField]
    private LayerMask groundLayer;

    private float rotationSmoothVelocity;

    private float speed;
    private bool isGrounded;


    private Rigidbody rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        speed = walkSpeed;

    }
    private void Start()
    {
        inputManager.OnMoveInput += Move;
        inputManager.OnSprintInput += Sprint;
        inputManager.OnJumpInput += Jump;
    }

    private void OnDestroy()
    {
        inputManager.OnMoveInput -= Move;
        inputManager.OnSprintInput -= Sprint;
        inputManager.OnJumpInput -= Jump;
    }

    private void Update()
    {
        CheckIsGrounded();
    }

    private void Move(Vector2 axisDirection)
    {

        if(axisDirection.magnitude >= 0.1)
        {
            float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

            Vector3 movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
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
        }

        
    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundDetector.position, detectorRadius, groundLayer);
    }
}
