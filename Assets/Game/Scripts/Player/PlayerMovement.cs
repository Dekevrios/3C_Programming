using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private InputManager inputManager;

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
        Vector3 movementDirection = new Vector3(axisDirection.x, 0, axisDirection.y);
        rb.AddForce(movementDirection * moveSpeed * Time.deltaTime);

    }
}
