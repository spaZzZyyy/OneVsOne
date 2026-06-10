using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))] 
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float gravity = -9.81f;
    
    [Header("Look Settings")]
    [SerializeField] Transform cameraTransform; // Drag your Main Camera here in the Inspector
    [SerializeField] float mouseSensitivity = 0.1f;
    [SerializeField] float upperLookLimit = -80f;
    [SerializeField] float lowerLookLimit = 80f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float cameraVerticalRotation = 0f;

    // FIX: Read directly from the incoming 'value' parameter using .Get<Vector2>()
    private void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    private void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void OnJump(InputValue value)
    {
        Debug.Log("Jump");
        // Only jump if the character controller is firmly touching the floor
        if (characterController.isGrounded)
        {
            // Physics formula to calculate precise velocity needed to reach jumpHeight: 
            // Velocity = SquareRoot(JumpHeight * -2 * Gravity)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleMovement()
    {
        // Ground check for gravity resets
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Slight downward force to keep grounded firmly
        }

        // Calculate movement direction relative to where the player is currently facing
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Apply constant gravity over time
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // 1. Horizontal rotation: Turn the whole player body left/right
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // 2. Vertical rotation: Look up/down with the camera only
        cameraVerticalRotation -= lookInput.y * mouseSensitivity;
        
        // Clamp the vertical look angle so the player can't look back past their spine
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, upperLookLimit, lowerLookLimit);
        
        cameraTransform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
    }
}