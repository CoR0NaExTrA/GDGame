using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        movement = inputActions.Player.Move.ReadValue<Vector2>();
        movement = movement.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }
}
