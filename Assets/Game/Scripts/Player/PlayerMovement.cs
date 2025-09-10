using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private float rotationSmoothTime = 0.1f;
    private float rotationSmoothVelocity;


    private Rigidbody rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        inputManager.OnMoveInput += Move;
         
    }

    private void OnDestroy()
    {
        inputManager.OnMoveInput -= Move;
    }

    private void Move(Vector2 axisDirection)
    {

        if(axisDirection.magnitude >= 0.1)
        {
            float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

            Vector3 movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
            rb.AddForce(movementDirection * moveSpeed * Time.deltaTime);
        }

    }
}
